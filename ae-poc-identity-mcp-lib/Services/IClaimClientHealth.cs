using Ae.Poc.Identity.Mcp.Dtos;

namespace Ae.Poc.Identity.Mcp.Services;

/// <summary>
/// Interface for checking the health of the Claim Client's dependencies.
/// </summary>
public interface IClaimClientHealth
{
    /// <summary>
    /// Checks the health of the dependent service asynchronously.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A DTO containing readiness status and metadata.</returns>
    Task<DependencyHealthDto> GetHealthAsync(CancellationToken ct = default);
}
