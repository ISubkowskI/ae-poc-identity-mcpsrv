using System.Text.Json.Serialization;

namespace Ae.Poc.Identity.Mcp.Dtos
{
    public sealed record ClaimsInfoOutgoingDto
    {
        [JsonPropertyName("count")]
        public int TotalCount { get; init; } = 0;
    }
}
