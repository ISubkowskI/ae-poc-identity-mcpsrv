using Ae.Poc.Identity.Mcp.Authentication;
using Ae.Poc.Identity.Mcp.Extensions;
using Ae.Poc.Identity.Mcp.Profiles;
using Ae.Poc.Identity.Mcp.Services;
using Ae.Poc.Identity.Mcp.Settings;
using Ae.Poc.Identity.Mcp.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Net;
using System.Reflection;


const string ConfigBaseName = "mcpsrvidentitysettings";

var logConfig = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console(standardErrorFromLevel: Serilog.Events.LogEventLevel.Verbose);
Log.Logger = logConfig.CreateBootstrapLogger();

try
{
    Log.Information("App starting ... '{Env}'", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty);
    Log.Debug("Working directory: '{CurrentDirectory}'", Environment.CurrentDirectory);
    var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    var builder = WebApplication.CreateBuilder(args);
    Log.Verbose("\n    Content Root Path: '{ContentRootPath}'\n    Directory Current: '{GetCurrentDirectory}'\n    Builder Env: '{BuilderEnv}'",
        builder.Environment.ContentRootPath, Directory.GetCurrentDirectory(), builder.Environment.EnvironmentName);

    // Determine external config directory from command-line, environment variable, or default location
    // Priority: 1) --configpath=  2) CONFIG_PATH environment variable  3) default current directory
    string configDir = ConfigurationHelper.ResolveConfigDirectory(args, builder.Environment.ContentRootPath ?? Directory.GetCurrentDirectory());
    if (!Directory.Exists(configDir))
    {
        Log.Fatal("Required configuration directory was not found: {ConfigDir}", configDir);
        return 2;
    }
    Log.Information("Configuration use folder: {ConfigDir}", configDir);
   
    // Explicitly configure configuration sources from external directory
    builder.Configuration.Sources.Clear();
    builder.Configuration
        .SetBasePath(configDir)
        .AddJsonFile(ConfigBaseName + ".json", optional: false, reloadOnChange: true)
        .AddJsonFile($"{ConfigBaseName}.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

    // Add environment variables and command line arguments last so they can override file settings
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddCommandLine(args);

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