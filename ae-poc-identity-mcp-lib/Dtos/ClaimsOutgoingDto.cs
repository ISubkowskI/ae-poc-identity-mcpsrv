using System.Text.Json.Serialization;

namespace Ae.Poc.Identity.Mcp.Dtos
{
    public sealed record ClaimsOutgoingDto
    {
        [JsonPropertyName("claims")]
        public IEnumerable<ClaimOutgoingDto> Claims { get; init; } = Array.Empty<ClaimOutgoingDto>();

        [JsonPropertyName("claimsinfo")]
        public ClaimsInfoOutgoingDto? ClaimsInfo { get; init; }
    }
}
