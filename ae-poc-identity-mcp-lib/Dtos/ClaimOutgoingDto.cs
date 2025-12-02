using System.Text.Json.Serialization;

namespace Ae.Poc.Identity.Mcp.Dtos;

public sealed record ClaimOutgoingDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; } = Guid.Empty;
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;
    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;
    [JsonPropertyName("valueType")]
    public string ValueType { get; init; } = string.Empty;
    [JsonPropertyName("displayText")]
    public string DisplayText { get; init; } = string.Empty;
    [JsonPropertyName("properties")]
    public IDictionary<string, string>? Properties { get; init; }
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
