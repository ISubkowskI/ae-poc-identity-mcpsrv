using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ae.Poc.Identity.Mcp.Dtos
{
    public sealed record ClaimsQueryIncomingDto
    {
        [Description("The id of the claim to get details for.")]
        public int Skipped { get; init; } = 0;

    //    [IntegerValidator(MinValue = 1, MaxValue = 100,
    //ExcludeRange = true)]
    //    [Max(100, ErrorMessage = "The Description cannot exceed 500 characters.")]
        [Description("The id of the claim to get details for.")]
        public int NumberOf { get; init; } = 50;

        [Description("Whether to include claims info in the response. Optional parameter.")]
        public bool WithClaimsInfo { get; init; } = false;
    }
}
