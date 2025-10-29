using System.ComponentModel.DataAnnotations;

namespace Ae.Poc.Identity.Mcp.Services;

public interface IDtoValidator
{
    bool TryValidate(object obj, out ICollection<ValidationResult> validationResults);
}
