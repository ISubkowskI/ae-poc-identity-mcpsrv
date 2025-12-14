using Ae.Poc.Identity.Mcp.Settings;
using Ae.Poc.Identity.Mcp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;

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
                ResponseWriter = WriteResponse
            });

            // Readiness: Checks everything
            var readyCheck = webapp.MapHealthChecks(healthOptions.ReadyPath, new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = WriteResponse
            });

            if (healthOptions.Port.HasValue)
            {
                liveCheck.RequireHost($"*:{healthOptions.Port}");
                readyCheck.RequireHost($"*:{healthOptions.Port}");
            }
        }

        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = context.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppOptions>>().Value;

            var json = new
            {
                status = result.Status.ToString(),
                version = options.Version,
                clientId = options.ClientId,
                results = result.Entries.ToDictionary(
                    e => e.Key,
                    e => new
                    {
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        data = e.Value.Data
                    })
            };

            return context.Response.WriteAsJsonAsync(json);
        }

    }
}
