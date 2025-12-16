using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Ae.Poc.Identity.Mcp.Services;

public class ClaimClientHealthCheck : IHealthCheck
{
    private readonly IClaimClientHealth _claimClient;
    private readonly ILogger<ClaimClientHealthCheck> _logger;

    public ClaimClientHealthCheck(IClaimClientHealth claimClient, ILogger<ClaimClientHealthCheck> logger)
    {
        _claimClient = claimClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var dependencyHealthDto = await _claimClient.GetHealthAsync(cancellationToken).ConfigureAwait(false);
            
            var data = new Dictionary<string, object>
            {
                { "version", dependencyHealthDto.Version ?? "unknown" },
                { "clientId", dependencyHealthDto.ClientId ?? "unknown" }
            };

            if (dependencyHealthDto.IsReady)
            {
                return HealthCheckResult.Healthy("Claim API is reachable and ready.", data);
            }
            
            return HealthCheckResult.Unhealthy("Claim API is not ready.", data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Claim API health check failed.");
            return HealthCheckResult.Unhealthy("Claim API is unreachable.", ex);
        }
    }
}
