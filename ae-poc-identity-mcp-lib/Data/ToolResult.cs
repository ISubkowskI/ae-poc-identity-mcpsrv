using Ae.Poc.Identity.Mcp.Dtos;

namespace Ae.Poc.Identity.Mcp.Data
{
    public sealed record ToolResult<T, R>
    {
        public bool IsSuccess { get; init; }
        public T? Value { get; init; }
        public R? Error { get; init; }
    }
}
