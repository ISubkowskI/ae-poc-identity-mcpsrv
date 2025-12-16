using Ae.Poc.Identity.Mcp.Data;

namespace Ae.Poc.Identity.Mcp.Services;

public interface IClaimClient
{
    /// <summary>
    /// Retrieves summary information about claims.
    /// </summary>
    Task<ClaimsInfo?> GetClaimsInfoAsync(CancellationToken ct = default);

    /// <summary>
    /// Loads claims based on the provided query.
    /// </summary>
    Task<IEnumerable<AppClaim>?> LoadClaimsAsync(ClaimsQuery claimsQuery, CancellationToken ct = default);

    /// <summary>
    /// Loads detailed information for a specific claim.
    /// </summary>
    Task<AppClaim?> LoadClaimDetailsAsync(string claimId, CancellationToken ct = default);

    /// <summary>
    /// Deletes a specific claim.
    /// </summary>
    Task<AppClaim> DeleteClaimAsync(string claimId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new claim.
    /// </summary>
    Task<AppClaim> CreateClaimAsync(AppClaim appClaim, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing claim.
    /// </summary>
    Task<AppClaim> UpdateClaimAsync(string claimId, AppClaim appClaim, CancellationToken ct = default);
}
