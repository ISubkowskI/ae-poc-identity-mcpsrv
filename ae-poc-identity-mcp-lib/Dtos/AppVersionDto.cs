namespace Ae.Poc.Identity.Mcp.Dtos;

public sealed record AppVersionDto
{
    public string AppVersion { get; init; } = string.Empty;
    public DateTimeOffset AppNow { get; init; }
    public DateTimeOffset AppNowUtc { get; init; }
    public long AppUtcTicks { get; init; }
}
