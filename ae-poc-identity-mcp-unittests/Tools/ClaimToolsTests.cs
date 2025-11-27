using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Services;
using Ae.Poc.Identity.Mcp.Tools;
using AutoMapper;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace Ae.Poc.Identity.Mcp.Unittests.Tools;

public class ClaimToolsTests
{
    private readonly Mock<IClaimClient> _mockClaimClient;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDtoValidator> _mockValidator;

    public ClaimToolsTests()
    {
        _mockClaimClient = new Mock<IClaimClient>();
        _mockMapper = new Mock<IMapper>();
        _mockValidator = new Mock<IDtoValidator>();
    }

    [Fact]
    public async Task GetClaimsAsync_ReturnsSerializedClaims_OnSuccess()
    {
        // Arrange
        var claims = new List<AppClaim> { new() { Id = Guid.NewGuid(), Type = "test" } };
        var dtos = new List<AppClaimOutgoingDto> { new() { Id = claims[0].Id, Type = "test" } };

        _mockClaimClient.Setup(c => c.LoadClaimsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);
        _mockMapper.Setup(m => m.Map<IEnumerable<AppClaimOutgoingDto>>(claims))
            .Returns(dtos);

        // Act
        var result = await ClaimTools.GetClaimsAsync(_mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.NotNull(result.Value);
        Assert.Contains(dtos[0].Id.ToString(), result.Value.First().Id.ToString());
    }

    [Fact]
    public async Task GetClaimsAsync_ReturnsError_OnException()
    {
        // Arrange
        _mockClaimClient.Setup(c => c.LoadClaimsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await ClaimTools.GetClaimsAsync(_mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Test exception", result.Error.Errors?.First());
        Assert.Contains("Error", result.Error.Status);
    }

    [Fact]
    public async Task CreateClaimAsync_ReturnsValidationFailed_WhenValidatorFails()
    {
        // Arrange
        var dto = new AppClaimCreateDto();
        ICollection<ValidationResult> validationResults = [new ValidationResult("Invalid")];

        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(false);

        // Act
        var result = await ClaimTools.CreateClaimAsync(dto, _mockClaimClient.Object, _mockMapper.Object, _mockValidator.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Validation Failed", result.Error.Status);
        Assert.Contains("Invalid", result.Error.Errors?.First());
    }

    [Fact]
    public async Task CreateClaimAsync_ReturnsCreatedClaim_OnSuccess()
    {
        // Arrange
        var dto = new AppClaimCreateDto { Type = "test" };
        var claim = new AppClaim { Type = "test" };
        var createdClaim = new AppClaim { Id = Guid.NewGuid(), Type = "test" };
        var outgoingDto = new AppClaimOutgoingDto { Id = createdClaim.Id, Type = "test" };
        ICollection<ValidationResult> validationResults = [];

        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(true);
        _mockMapper.Setup(m => m.Map<AppClaim>(dto)).Returns(claim);
        _mockClaimClient.Setup(c => c.CreateClaimAsync(claim, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdClaim);
        _mockMapper.Setup(m => m.Map<AppClaimOutgoingDto>(createdClaim)).Returns(outgoingDto);

        // Act
        var result = await ClaimTools.CreateClaimAsync(dto, _mockClaimClient.Object, _mockMapper.Object, _mockValidator.Object);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
        Assert.Contains(outgoingDto.Id.ToString(), result.Value.Id.ToString());
    }

    [Fact]
    public async Task GetClaimsAsync_ReturnsWarning_WhenClaimsIsNull()
    {
        // Arrange
        _mockClaimClient.Setup(c => c.LoadClaimsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<AppClaim>)null!);

        // Act
        var result = await ClaimTools.GetClaimsAsync(_mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("No claims found", result.Error.Errors?.First());
        Assert.Contains("Warning", result.Error.Status);
    }

    [Fact]
    public async Task GetClaimsAsync_HandlesNullMapperResult_Gracefully()
    {
        // Arrange
        var claims = new List<AppClaim> { new() { Id = Guid.NewGuid(), Type = "test" } };
        _mockClaimClient.Setup(c => c.LoadClaimsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        // Mapper returns null
        _mockMapper.Setup(m => m.Map<IEnumerable<AppClaimOutgoingDto>>(claims))
            .Returns((IEnumerable<AppClaimOutgoingDto>)null!);

        // Act
        var result = await ClaimTools.GetClaimsAsync(
            _mockClaimClient.Object,
            _mockMapper.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task GetClaimDetailsAsync_ReturnsClaimDetails_OnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var claim = new AppClaim { Id = id, Type = "test" };
        var dto = new AppClaimOutgoingDto { Id = id, Type = "test" };

        _mockClaimClient.Setup(c => c.LoadClaimDetailsAsync(id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(claim);
        _mockMapper.Setup(m => m.Map<AppClaimOutgoingDto>(claim))
            .Returns(dto);

        // Act
        var result = await ClaimTools.GetClaimDetailsAsync(id.ToString(), _mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
        Assert.Equal(id, result.Value.Id);
    }

    [Fact]
    public async Task GetClaimDetailsAsync_ReturnsValidationFailed_WhenClaimIdIsInvalid()
    {
        // Arrange
        var invalidId = "not-a-guid";

        // Act
        var result = await ClaimTools.GetClaimDetailsAsync(invalidId, _mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Validation Failed", result.Error.Status);
        Assert.Contains("must be a valid GUID", result.Error.Errors?.First());
    }

    [Theory]
    [InlineData("", "cannot be empty")]
    [InlineData("   ", "cannot be empty")]
    [InlineData("not-a-guid", "must be a valid GUID")]
    [InlineData("12345", "must be a valid GUID")]
    [InlineData("00000000-0000-0000-0000-000000000000", "cannot be an empty GUID")]
    public async Task GetClaimDetailsAsync_ReturnsValidationFailed_ForInvalidClaimIds(
    string invalidId,
    string expectedErrorFragment)
    {
        // Act
        var result = await ClaimTools.GetClaimDetailsAsync(
            invalidId,
            _mockClaimClient.Object,
            _mockMapper.Object);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Validation Failed", result.Error.Status);
        Assert.Contains(expectedErrorFragment, result.Error.Errors?.First());
    }

    [Fact]
    public async Task GetClaimDetailsAsync_ReturnsValidationFailed_WhenClaimIdIsEmpty()
    {
        // Arrange
        var emptyId = "";

        // Act
        var result = await ClaimTools.GetClaimDetailsAsync(emptyId, _mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Validation Failed", result.Error.Status);
        Assert.Contains("cannot be empty", result.Error.Errors?.First());
    }

    [Fact]
    public async Task GetClaimDetailsAsync_ReturnsWarning_WhenClaimNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockClaimClient.Setup(c => c.LoadClaimDetailsAsync(id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppClaim)null!);

        // Act
        var result = await ClaimTools.GetClaimDetailsAsync(id.ToString(), _mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Claim not found", result.Error.Errors?.First());
        Assert.Contains("Warning", result.Error.Status);
    }

    [Fact]
    public async Task GetClaimDetailsAsync_ReturnsError_OnException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockClaimClient.Setup(c => c.LoadClaimDetailsAsync(id.ToString(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await ClaimTools.GetClaimDetailsAsync(id.ToString(), _mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Database error", result.Error.Errors?.First());
        Assert.Contains("Error", result.Error.Status);
    }

    [Fact]
    public async Task DeleteClaimAsync_ReturnsDeletedClaim_OnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var deletedClaim = new AppClaim { Id = id, Type = "test" };
        var dto = new AppClaimOutgoingDto { Id = id, Type = "test" };

        _mockClaimClient.Setup(c => c.DeleteClaimAsync(id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedClaim);
        _mockMapper.Setup(m => m.Map<AppClaimOutgoingDto>(deletedClaim))
            .Returns(dto);

        // Act
        var result = await ClaimTools.DeleteClaimAsync(id.ToString(), _mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
        Assert.Equal(id, result.Value.Id);
    }

    [Fact]
    public async Task DeleteClaimAsync_ReturnsValidationFailed_WhenClaimIdIsInvalid()
    {
        // Arrange
        var invalidId = "invalid-guid";

        // Act
        var result = await ClaimTools.DeleteClaimAsync(invalidId, _mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Validation Failed", result.Error.Status);
        Assert.Contains("must be a valid GUID", result.Error.Errors?.First());
    }

    [Fact]
    public async Task DeleteClaimAsync_ReturnsValidationFailed_WhenClaimIdIsEmpty()
    {
        // Arrange
        var emptyId = "";

        // Act
        var result = await ClaimTools.DeleteClaimAsync(emptyId, _mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Validation Failed", result.Error.Status);
        Assert.Contains("cannot be empty", result.Error.Errors?.First());
    }

    [Fact]
    public async Task DeleteClaimAsync_ReturnsError_OnException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockClaimClient.Setup(c => c.DeleteClaimAsync(id.ToString(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await ClaimTools.DeleteClaimAsync(id.ToString(), _mockClaimClient.Object, _mockMapper.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Delete failed", result.Error.Errors?.First());
        Assert.Contains("Error", result.Error.Status);
    }

    [Fact]
    public async Task UpdateClaimAsync_ReturnsUpdatedClaim_OnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new AppClaimUpdateDto { Id = id, Type = "updated", Value = "test", ValueType = "string", DisplayText = "Test" };
        var claim = new AppClaim { Id = id, Type = "updated" };
        var updatedClaim = new AppClaim { Id = id, Type = "updated" };
        var outgoingDto = new AppClaimOutgoingDto { Id = id, Type = "updated" };
        ICollection<ValidationResult> validationResults = [];

        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(true);
        _mockMapper.Setup(m => m.Map<AppClaim>(dto)).Returns(claim);
        _mockClaimClient.Setup(c => c.UpdateClaimAsync(id.ToString(), claim, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedClaim);
        _mockMapper.Setup(m => m.Map<AppClaimOutgoingDto>(updatedClaim)).Returns(outgoingDto);

        // Act
        var result = await ClaimTools.UpdateClaimAsync(id.ToString(), dto, _mockClaimClient.Object, _mockMapper.Object, _mockValidator.Object);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
        Assert.Equal(id, result.Value.Id);
    }

    [Fact]
    public async Task UpdateClaimAsync_ReturnsValidationFailed_WhenValidatorFails()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new AppClaimUpdateDto { Id = id };
        ICollection<ValidationResult> validationResults = [new ValidationResult("Type is required")];

        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(false);

        // Act
        var result = await ClaimTools.UpdateClaimAsync(id.ToString(), dto, _mockClaimClient.Object, _mockMapper.Object, _mockValidator.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Validation Failed", result.Error.Status);
        Assert.Contains("Type is required", result.Error.Errors?.First());
    }

    [Fact]
    public async Task UpdateClaimAsync_ReturnsValidationFailed_WhenClaimIdIsInvalid()
    {
        // Arrange
        var invalidId = "not-a-guid";
        var dto = new AppClaimUpdateDto { Id = Guid.NewGuid() };

        // Act
        var result = await ClaimTools.UpdateClaimAsync(invalidId, dto, _mockClaimClient.Object, _mockMapper.Object, _mockValidator.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Validation Failed", result.Error.Status);
        Assert.Contains("must be a valid GUID", result.Error.Errors?.First());
    }

    [Fact]
    public async Task UpdateClaimAsync_ReturnsValidationFailed_WhenIdMismatch()
    {
        // Arrange
        var pathId = Guid.NewGuid();
        var bodyId = Guid.NewGuid();
        var dto = new AppClaimUpdateDto { Id = bodyId, Type = "test", Value = "test", ValueType = "string", DisplayText = "Test" };

        // Act
        var result = await ClaimTools.UpdateClaimAsync(pathId.ToString(), dto, _mockClaimClient.Object, _mockMapper.Object, _mockValidator.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Validation Failed", result.Error.Status);
        Assert.Contains("must match", result.Error.Errors?.First());
    }

    [Fact]
    public async Task UpdateClaimAsync_ReturnsError_OnException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new AppClaimUpdateDto { Id = id, Type = "test", Value = "test", ValueType = "string", DisplayText = "Test" };
        var claim = new AppClaim { Id = id, Type = "test" };
        ICollection<ValidationResult> validationResults = [];

        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(true);
        _mockMapper.Setup(m => m.Map<AppClaim>(dto)).Returns(claim);
        _mockClaimClient.Setup(c => c.UpdateClaimAsync(id.ToString(), claim, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await ClaimTools.UpdateClaimAsync(id.ToString(), dto, _mockClaimClient.Object, _mockMapper.Object, _mockValidator.Object);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Update failed", result.Error.Errors?.First());
        Assert.Contains("Error", result.Error.Status);
    }
}
