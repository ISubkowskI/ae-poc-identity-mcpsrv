using Ae.Poc.Identity.Mcp.Data;

namespace Ae.Poc.Identity.Mcp.Services;

public interface IClaimClient
{
    Task<IEnumerable<AppClaim>> LoadClaimsAsync(CancellationToken ct = default);
    Task<AppClaim> LoadClaimDetailsAsync(string claimId, CancellationToken ct = default);
    Task<AppClaim> DeleteClaimAsync(string claimId, CancellationToken ct = default);
    Task<AppClaim> CreateClaimAsync(AppClaim appClaim, CancellationToken ct = default);
    Task<AppClaim> UpdateClaimAsync(string claimId, AppClaim appClaim, CancellationToken ct = default);
}
