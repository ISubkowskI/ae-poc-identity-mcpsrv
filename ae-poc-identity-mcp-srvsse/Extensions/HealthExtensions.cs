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
            .AddCheck<ClaimClientHealthCheck>("identitystorage-api")
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

            var appOptions = context.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppOptions>>().Value;

            var json = new
            {
                status = result.Status.ToString(),
                results = result.Entries.ToDictionary(
                    e => e.Key,
                    e => {
                         var data = new Dictionary<string, object>(e.Value.Data);
                         object? versionObj = null;
                         object? clientIdObj = null;

                         if (e.Key == "self")
                         {
                             versionObj = appOptions.Version;
                             clientIdObj = appOptions.ClientId;
                         }
                         else
                         {
                             data.Remove("version", out versionObj);
                             data.Remove("clientId", out clientIdObj);
                         }

                         return new
                         {
                             status = e.Value.Status.ToString(),
                             description = e.Value.Description,
                             duration = e.Value.Duration,
                             version = versionObj,
                             clientId = clientIdObj,
                             data = data
                         };
                    })
            };

            return context.Response.WriteAsJsonAsync(json);
        }

    }
}
