using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using Xunit;

namespace Ae.Poc.Identity.Mcp.Unittests.Data;

public class ToolResultFactoryTests
{
    [Fact]
    public void Success_ReturnsSuccessResult_WithValue()
    {
        // Arrange
        var value = new AppClaimOutgoingDto { Id = Guid.NewGuid(), Type = "test" };

        // Act
        var result = ToolResultFactory.Success(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
        Assert.Equal(value.Id, result.Value.Id);
    }

    [Fact]
    public void ValidationFailed_ReturnsFailureResult_WithErrors()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var result = ToolResultFactory.ValidationFailed<AppClaimOutgoingDto>(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Equal("Validation Failed", result.Error.Status);
        Assert.Equal(2, result.Error.Errors?.Count());
        Assert.Contains("Error 1", result.Error.Errors);
        Assert.Contains("Error 2", result.Error.Errors);
    }

    [Fact]
    public void ValidationFailed_ThrowsArgumentException_WhenErrorsIsEmpty()
    {
        // Arrange
        var errors = Array.Empty<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ToolResultFactory.ValidationFailed<AppClaimOutgoingDto>(errors));
        Assert.Contains("must contain at least one message", exception.Message);
    }

    [Fact]
    public void ValidationFailed_ThrowsArgumentException_WhenErrorsIsNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ToolResultFactory.ValidationFailed<AppClaimOutgoingDto>(null!));
        Assert.Contains("must contain at least one message", exception.Message);
    }

    [Fact]
    public void ValidationFailed_HandlesNullErrorMessages()
    {
        // Arrange
        var errors = new string?[] { "Valid error", null, "Another error" };

        // Act
        var result = ToolResultFactory.ValidationFailed<AppClaimOutgoingDto>(errors!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("Valid error", result.Error.Errors);
        Assert.Contains("Unknown validation warning.", result.Error.Errors);
        Assert.Contains("Another error", result.Error.Errors);
    }

    [Fact]
    public void Warning_ReturnsFailureResult_WithWarningMessage()
    {
        // Arrange
        var message = "This is a warning";

        // Act
        var result = ToolResultFactory.Warning<AppClaimOutgoingDto>(message);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Equal("Warning", result.Error.Status);
        Assert.Single(result.Error.Errors);
        Assert.Contains(message, result.Error.Errors);
    }

    [Fact]
    public void Warning_HandlesNullMessage()
    {
        // Act
        var result = ToolResultFactory.Warning<AppClaimOutgoingDto>(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("Unknown warning message.", result.Error.Errors);
    }

    [Fact]
    public void Failure_ReturnsFailureResult_WithErrors()
    {
        // Arrange
        var errors = new[] { "Critical error", "Another error" };

        // Act
        var result = ToolResultFactory.Failure<AppClaimOutgoingDto>(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Equal("Error", result.Error.Status);
        Assert.Equal(2, result.Error.Errors?.Count());
        Assert.Contains("Critical error", result.Error.Errors);
    }

    [Fact]
    public void Failure_ThrowsArgumentException_WhenErrorsIsEmpty()
    {
        // Arrange
        var errors = Array.Empty<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ToolResultFactory.Failure<AppClaimOutgoingDto>(errors));
        Assert.Contains("must contain at least one error message", exception.Message);
    }

    [Fact]
    public void Failure_ThrowsArgumentException_WhenErrorsIsNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            ToolResultFactory.Failure<AppClaimOutgoingDto>(null!));
        Assert.Contains("must contain at least one error message", exception.Message);
    }

    [Fact]
    public void Failure_HandlesNullErrorMessages()
    {
        // Arrange
        var errors = new string?[] { null, "Valid error" };

        // Act
        var result = ToolResultFactory.Failure<AppClaimOutgoingDto>(errors!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("Unknown error.", result.Error.Errors);
        Assert.Contains("Valid error", result.Error.Errors);
    }

    [Fact]
    public void FromException_ReturnsFailureResult_WithExceptionMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        var result = ToolResultFactory.FromException<AppClaimOutgoingDto>(exception);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Equal("Error", result.Error.Status);
        Assert.Single(result.Error.Errors);
        Assert.Contains("Something went wrong", result.Error.Errors);
    }

    [Fact]
    public void FromException_HandlesNullException()
    {
        // Act
        var result = ToolResultFactory.FromException<AppClaimOutgoingDto>(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("Unknown exception message.", result.Error.Errors);
    }

    [Fact]
    public void ValidationFailed_AcceptsCustomStatus()
    {
        // Arrange
        var errors = new[] { "Custom validation error" };
        var customStatus = "Custom Validation Status";

        // Act
        var result = ToolResultFactory.ValidationFailed<AppClaimOutgoingDto>(errors, customStatus);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(customStatus, result.Error.Status);
    }

    [Fact]
    public void Warning_AcceptsCustomStatus()
    {
        // Arrange
        var message = "Warning message";
        var customStatus = "Custom Warning";

        // Act
        var result = ToolResultFactory.Warning<AppClaimOutgoingDto>(message, customStatus);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(customStatus, result.Error.Status);
    }

    [Fact]
    public void Failure_AcceptsCustomStatus()
    {
        // Arrange
        var errors = new[] { "Error message" };
        var customStatus = "Custom Error";

        // Act
        var result = ToolResultFactory.Failure<AppClaimOutgoingDto>(errors, customStatus);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(customStatus, result.Error.Status);
    }
}
