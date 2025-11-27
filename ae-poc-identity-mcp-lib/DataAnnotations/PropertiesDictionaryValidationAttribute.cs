using System.ComponentModel.DataAnnotations;

namespace Ae.Poc.Identity.Mcp.DataAnnotations
{
    public sealed class PropertiesDictionaryValidationAttribute : ValidationAttribute
    {
        public int MaxItems { get; set; } = 50;
        public int MaxKeyLength { get; set; } = 100;
        public int MaxValueLength { get; set; } = 500;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not IDictionary<string, string> properties)
                return ValidationResult.Success;
            if (properties.Count > MaxItems)
                return new ValidationResult($"Properties cannot exceed {MaxItems} items.");
            foreach (var kvp in properties)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                    return new ValidationResult("Property keys cannot be empty.");
                if (kvp.Key.Length > MaxKeyLength)
                    return new ValidationResult($"Property keys cannot exceed {MaxKeyLength} characters.");
                if (kvp.Value?.Length > MaxValueLength)
                    return new ValidationResult($"Property values cannot exceed {MaxValueLength} characters.");
            }
            return ValidationResult.Success;
        }
    }
}
