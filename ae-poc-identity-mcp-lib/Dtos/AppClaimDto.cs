namespace Ae.Poc.Identity.Mcp.Dtos;

public sealed record AppClaimDto
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Type { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string ValueType { get; init; } = string.Empty;
    public string DisplayText { get; init; } = string.Empty;
    public IDictionary<string, string>? Properties { get; init; }
    public string? Description { get; init; }
}
