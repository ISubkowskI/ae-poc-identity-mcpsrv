﻿using System.ComponentModel.DataAnnotations;

namespace Ae.Poc.Identity.Mcp.Services;

/// <summary>
/// Provides a concrete implementation of <see cref="IDtoValidator"/>
/// using <see cref="System.ComponentModel.DataAnnotations.Validator"/>.
/// This class is responsible for validating DTOs based on their data annotations.
/// </summary>
public sealed class DtoValidator : IDtoValidator
{
    /// <summary>
    /// Validates the specified object instance using data annotations.
    /// </summary>
    /// <param name="obj">The object instance to validate. Cannot be null.</param>
    /// <param name="validationResults">When this method returns, contains a collection of <see cref="ValidationResult"/> objects that describe any validation errors. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the <paramref name="obj"/> is valid; otherwise, <c>false</c>.</returns>
    public bool TryValidate(object obj, out ICollection<ValidationResult> validationResults)
    {
        var validationContext = new ValidationContext(obj, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(obj, validationContext, results, validateAllProperties: true);
        validationResults = results;
        return isValid;
    }
}
