using Ae.Poc.Identity.Mcp.Dtos;

namespace Ae.Poc.Identity.Mcp.Services;

public interface IClaimClientHealth
{
    Task<DependencyHealthDto> GetHealthAsync(CancellationToken ct = default);
}
