using Ae.Poc.Identity.Mcp.Authentication;
using Ae.Poc.Identity.Mcp.Profiles;
using Ae.Poc.Identity.Mcp.Services;
using Ae.Poc.Identity.Mcp.Settings;
using Ae.Poc.Identity.Mcp.SrvSse.Services;
using Ae.Poc.Identity.Mcp.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
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
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services
        .Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.App))
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

    if (!appOptions.DisableHealthChecks)
    {
        builder.Services.AddHealthChecks()
            .AddCheck<ClaimClientHealthCheck>("claim-api")
            .AddCheck("self", () => HealthCheckResult.Healthy());
    }

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

    if (!appOptions.DisableHealthChecks)
    {
        webapp.MapHealthChecks(appOptions.HealthCheckPath, new HealthCheckOptions
        {
            // Ensure we run *all* checks (default Behavior) 
            Predicate = _ => true,
        });
    }
    webapp.MapMcp(appOptions.MapMcpPattern)
        .RequireAuthorization(); // Protect the MCP endpoint

    if (!string.IsNullOrWhiteSpace(appOptions.Url))
    {
        await webapp.RunAsync(appOptions.Url);
    }
    else
    {
        await webapp.RunAsync();
    }
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