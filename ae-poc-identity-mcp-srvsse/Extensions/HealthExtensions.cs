using Ae.Poc.Identity.Mcp.Settings;
using Ae.Poc.Identity.Mcp.SrvSse.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ae.Poc.Identity.Mcp.Extensions
{
    internal static class HealthExtensions
    {
        internal static IHealthChecksBuilder AddApplicationHealthChecks(this IServiceCollection services) =>
          services.AddHealthChecks()
            .AddCheck<ClaimClientHealthCheck>("claim-api")
            .AddCheck("self", () => HealthCheckResult.Healthy());

        internal static void MapApplicationHealthChecks(this WebApplication webapp, HealthOptions healthOptions)
        {
            // Liveness: Checks only "self"
            var liveCheck = webapp.MapHealthChecks(healthOptions.LivePath, new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self"),
                ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
            });

            // Readiness: Checks everything
            var readyCheck = webapp.MapHealthChecks(healthOptions.ReadyPath, new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
            });

            if (healthOptions.Port.HasValue)
            {
                liveCheck.RequireHost($"*:{healthOptions.Port}");
                readyCheck.RequireHost($"*:{healthOptions.Port}");
            }
        }

    }
}
