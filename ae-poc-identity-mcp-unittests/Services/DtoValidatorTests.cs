using Ae.Poc.Identity.Mcp.Services;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Ae.Poc.Identity.Mcp.Unittests.Services;

public class DtoValidatorTests
{
    private readonly DtoValidator _validator;

    public DtoValidatorTests()
    {
        _validator = new DtoValidator();
    }

    [Fact]
    public void TryValidate_ReturnsTrue_WhenObjectIsValid()
    {
        // Arrange
        var model = new TestModel { Name = "Valid" };

        // Act
        var result = _validator.TryValidate(model, out var validationResults);

        // Assert
        Assert.True(result);
        Assert.Empty(validationResults);
    }

    [Fact]
    public void TryValidate_ReturnsFalse_WhenObjectIsInvalid()
    {
        // Arrange
        var model = new TestModel { Name = null! }; // Invalid

        // Act
        var result = _validator.TryValidate(model, out var validationResults);

        // Assert
        Assert.False(result);
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, r => r.ErrorMessage == "Name is required");
    }

    private class TestModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;
    }
}
