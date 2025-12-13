using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Ae.Poc.Identity.Mcp.Services;

public class ClaimClientHealthCheck : IHealthCheck
{
    private readonly IClaimClient _claimClient;
    private readonly ILogger<ClaimClientHealthCheck> _logger;

    public ClaimClientHealthCheck(IClaimClient claimClient, ILogger<ClaimClientHealthCheck> logger)
    {
        _claimClient = claimClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var info = await _claimClient.GetClaimsInfoAsync(cancellationToken).ConfigureAwait(false);
            if (info != null)
            {
                return HealthCheckResult.Healthy("Claim API is reachable.");
            }
            
            return HealthCheckResult.Unhealthy("Claim API returned null info.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Claim API health check failed.");
            return HealthCheckResult.Unhealthy("Claim API is unreachable.", ex);
        }
    }
}
