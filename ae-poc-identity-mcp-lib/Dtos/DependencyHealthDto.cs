namespace Ae.Poc.Identity.Mcp.Dtos;

public sealed record DependencyHealthDto
{
    public bool IsReady { get; init; }
    public string? Version { get; init; }
    public string? ClientId { get; init; }
}
