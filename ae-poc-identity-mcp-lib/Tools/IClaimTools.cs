using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;

namespace Ae.Poc.Identity.Mcp.Tools;

/// <summary>
/// Interface for ClaimTools to support dependency injection and testing.
/// </summary>
public interface IClaimTools
{
    Task<ToolResult<ClaimsOutgoingDto, ErrorOutgoingDto>> GetClaimsAsync(ClaimsQueryIncomingDto queryIncomingDto, CancellationToken ct = default);
    Task<ToolResult<ClaimOutgoingDto, ErrorOutgoingDto>> GetClaimDetailsAsync(string claimId, CancellationToken ct = default);
    Task<ToolResult<ClaimOutgoingDto, ErrorOutgoingDto>> DeleteClaimAsync(string claimId, CancellationToken ct = default);
    Task<ToolResult<ClaimOutgoingDto, ErrorOutgoingDto>> CreateClaimAsync(ClaimCreateDto claimDto, CancellationToken ct = default);
    Task<ToolResult<ClaimOutgoingDto, ErrorOutgoingDto>> UpdateClaimAsync(string claimId, ClaimUpdateDto claimDto, CancellationToken ct = default);
}
