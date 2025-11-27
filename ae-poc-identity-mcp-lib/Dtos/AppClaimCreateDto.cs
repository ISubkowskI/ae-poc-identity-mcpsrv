using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Ae.Poc.Identity.Mcp.DataAnnotations;

namespace Ae.Poc.Identity.Mcp.Dtos;

public sealed record AppClaimCreateDto
{
    [Required(ErrorMessage = "The Type field is required")]
    [Description("The type of the claim (e.g., 'email', 'role'). This field is mandatory.")]
    public string Type { get; init; } = string.Empty;

    [Required(ErrorMessage = "The Value field is required")]
    [Description("The value of the claim. This field is mandatory.")]
    public string Value { get; init; } = string.Empty;

    [Required(ErrorMessage = "The ValueType field is required.")]
    [Description("The XML schema data type of the value (e.g., 'http://www.w3.org/2001/XMLSchema#string'). This field is mandatory.")]
    public string ValueType { get; init; } = string.Empty;

    [Required(ErrorMessage = "The DisplayText field is required.")]
    [Description("A human-readable display text for the claim. This field is mandatory.")]
    public string DisplayText { get; init; } = string.Empty;

    [PropertiesDictionaryValidation(MaxItems = 50, MaxKeyLength = 100, MaxValueLength = 500)]
    [Description("An optional dictionary of additional properties for the claim (string key-value pairs).")]
    public IDictionary<string, string>? Properties { get; init; }

    [MaxLength(500, ErrorMessage = "The Description cannot exceed 500 characters.")]
    [Description("An optional description for the claim (max 500 characters).")]
    public string? Description { get; init; }
}
