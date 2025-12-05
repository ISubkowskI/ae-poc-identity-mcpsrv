using Ae.Poc.Identity.Mcp.Authentication;
using Ae.Poc.Identity.Mcp.Extensions;
using Ae.Poc.Identity.Mcp.Profiles;
using Ae.Poc.Identity.Mcp.Services;
using Ae.Poc.Identity.Mcp.Settings;
using Ae.Poc.Identity.Mcp.SrvSse.Services;
using Ae.Poc.Identity.Mcp.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Net;
using System.Reflection;

var logConfig = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console(standardErrorFromLevel: Serilog.Events.LogEventLevel.Verbose);
Log.Logger = logConfig.CreateBootstrapLogger();

try
{
    Log.Debug("App starting ... '{Env}'", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty);
    Log.Debug("Working directory: '{CurrentDirectory}'", Environment.CurrentDirectory);
    var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    var builder = WebApplication.CreateBuilder(args);

    // Explicitly configure configuration sources
    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args);
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services
        .Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.App))
        .Configure<HealthOptions>(builder.Configuration.GetSection(HealthOptions.Health))
        .Configure<ServerAuthenticationOptions>(builder.Configuration.GetSection(ServerAuthenticationOptions.Authentication))
        .Configure<IdentityStorageApiOptions>(builder.Configuration.GetSection(IdentityStorageApiOptions.IdentityStorageApi));

    builder.Services.AddAutoMapper(m =>
    {
        m.AddProfile<DataProfile>();
    });

    builder.Services.AddScoped<IDtoValidator, DtoValidator>();
    builder.Services.AddScoped<IClaimTools, ClaimTools>();
    builder.Services.AddHttpClient<IClaimClient, ClaimClient>()
        .ConfigureHttpClient(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            MaxConnectionsPerServer = 25
        });

    var appOptions = builder.Configuration.GetSection(AppOptions.App).Get<AppOptions>() ?? new AppOptions();
    var healthOptions = builder.Configuration.GetSection(HealthOptions.Health).Get<HealthOptions>() ?? new HealthOptions();

    Log.Information("{AppName} ver:{AppVersion}", appOptions.Name, appOptions.Version);

    // Add Authentication Services
    var srvAuthOptions = builder.Configuration.GetSection(ServerAuthenticationOptions.Authentication)
        .Get<ServerAuthenticationOptions>() ?? new ServerAuthenticationOptions();
    builder.Services.AddAuthentication(srvAuthOptions.Scheme)
        .AddScheme<AuthenticationSchemeOptions, ServerFixedTokenAuthenticationHandler>(srvAuthOptions.Scheme, options =>
        {
            options.TimeProvider = TimeProvider.System;
        });
    builder.Services.AddAuthorization();

    if (healthOptions.Enabled)
    {
        builder.Services.AddApplicationHealthChecks();
    }

    builder.WebHost.ConfigureKestrel((context, options) =>
    {
        // Listen on App Port
        if (appOptions.Uri != null)
        {
            if (string.Equals(appOptions.Uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                options.ListenLocalhost(appOptions.Uri.Port);
            }
            else if (IPAddress.TryParse(appOptions.Uri.Host, out var ip))
            {
                options.Listen(ip, appOptions.Uri.Port);
            }
            else
            {
                options.ListenAnyIP(appOptions.Uri.Port);
            }
        }
        else
        {
            // Default port if no Url is configured
            options.ListenAnyIP(AppOptions.DefaultPort);
        }

        // Listen on Health Port (9007)
        if (healthOptions.Enabled && healthOptions.Port.HasValue)
        {
            options.ListenAnyIP(healthOptions.Port.Value);
        }
    });

    // MCP Server Setup
    builder.Services
        .AddMcpServer(options =>
        {
            options.ServerInfo = new() { Name = appOptions.Name, Version = appOptions.Version };
            options.ServerInstructions = "Manage identity claims.";
        })
        .WithHttpTransport()
        .WithTools([typeof(ClaimTools), typeof(AppInfoTool)]);

    // 
    var webapp = builder.Build();
    webapp.UseAuthentication();
    webapp.UseAuthorization();

    if (healthOptions.Enabled)
    {
        webapp.MapApplicationHealthChecks(healthOptions);
    }
    webapp.MapMcp(appOptions.MapMcpPattern)
        .RequireAuthorization() // Protect the MCP endpoint
        .RequireHost($"*:{appOptions.Uri?.Port ?? AppOptions.DefaultPort}"); // Restrict to App Port

    await webapp.RunAsync();
    return 0;
}
catch (Exception exc)
{
    Log.Fatal(exc, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration testing with WebApplicationFactory
public partial class Program { }