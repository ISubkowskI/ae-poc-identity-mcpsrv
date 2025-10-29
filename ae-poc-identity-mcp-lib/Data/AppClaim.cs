namespace Ae.Poc.Identity.Mcp.Data;

public sealed class AppClaim
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty;
    public IDictionary<string, string>? Properties { get; set; }
    public string? Description { get; set; }
}

