namespace Ae.Poc.Identity.Mcp.Data
{
    public sealed record ClaimsQuery
    {
        public int Skipped { get; init; } = 0;

        public int NumberOf { get; init; } = 50;
    }
}
