using System.Text.Json.Serialization;

namespace Ae.Poc.Identity.Mcp.Dtos;

public sealed record AppVersionDto
{
    [JsonPropertyName("appVersion")]
    public string AppVersion { get; init; } = string.Empty;
    [JsonPropertyName("appNow")]
    public DateTimeOffset AppNow { get; init; }
    [JsonPropertyName("appNowUtc")]
    public DateTimeOffset AppNowUtc { get; init; }
    [JsonPropertyName("appUtcTicks")]
    public long AppUtcTicks { get; init; }
}
