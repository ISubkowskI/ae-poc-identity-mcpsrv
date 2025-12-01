using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Services;
using Ae.Poc.Identity.Mcp.Tools;
using Ae.Poc.Identity.Mcp.Profiles;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace Ae.Poc.Identity.Mcp.Unittests.Tools;

public class ClaimToolsTests
{
    private readonly Mock<IClaimClient> _mockClaimClient;
    private readonly IMapper _mapper;
    private readonly Mock<IDtoValidator> _mockValidator;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    
    public ClaimToolsTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(_mockLogger.Object);

        _mockClaimClient = new Mock<IClaimClient>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<DataProfile>(), _mockLoggerFactory.Object);
        _mapper = config.CreateMapper();
        _mockValidator = new Mock<IDtoValidator>();
       
    }

    [Fact]
    public async Task GetClaimsAsync_ReturnsSerializedClaims_OnSuccess()
    {
        // Arrange
        ClaimsQuery claimsQuery = new()
        {
            Skipped = 0,
            NumberOf = 10,
        };
        ClaimsQueryIncomingDto dto = new()
        {
            Skipped = 0,
            NumberOf = 10,
            WithClaimsInfo = true
        };
        var claims = new List<AppClaim> { new() { Id = Guid.NewGuid(), Type = "test" } };
        var dtos = new List<ClaimOutgoingDto> { new() { Id = claims[0].Id, Type = "test" } };

        ICollection<ValidationResult> validationResults = [];
        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(true);
        _mockClaimClient.Setup(c => c.LoadClaimsAsync(claimsQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);


        // Act
        var result = await ClaimTools.GetClaimsAsync(dto, _mockClaimClient.Object, _mapper, _mockValidator.Object, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Error.Should().BeNull();
        Assert.Contains(dtos[0].Id.ToString(), result.Value.Claims.First().Id.ToString());
    }

    [Fact]
    public async Task GetClaimsAsync_ReturnsError_OnException()
    {
        // Arrange
        ClaimsQuery claimsQuery = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        ClaimsQueryIncomingDto dto = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        _mockClaimClient.Setup(c => c.LoadClaimsAsync(claimsQuery, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        ICollection<ValidationResult> validationResults = [];
        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(true);



        // Act
        var result = await ClaimTools.GetClaimsAsync(dto, _mockClaimClient.Object, _mapper, _mockValidator.Object, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Error");
        result.Error.Errors.Should().Contain(e => e.Contains("Test exception"));
    }

    [Fact]
    public async Task CreateClaimAsync_ReturnsValidationFailed_WhenValidatorFails()
    {
        // Arrange
        var dto = new ClaimCreateDto();
        ICollection<ValidationResult> validationResults = [new ValidationResult("Invalid")];

        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(false);

        // Act
        var result = await ClaimTools.CreateClaimAsync(dto, _mockClaimClient.Object, _mapper, _mockValidator.Object, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Validation Failed");
        Assert.Contains("Invalid", result.Error.Errors?.First());
    }

    [Fact]
    public async Task CreateClaimAsync_ReturnsCreatedClaim_OnSuccess()
    {
        // Arrange
        var dto = new ClaimCreateDto { Type = "test" };
        var claim = new AppClaim { Type = "test" };
        var createdClaim = new AppClaim { Id = Guid.NewGuid(), Type = "test" };
        var outgoingDto = new ClaimOutgoingDto { Id = createdClaim.Id, Type = "test" };
        ICollection<ValidationResult> validationResults = [];

        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(true);

        _mockClaimClient.Setup(c => c.CreateClaimAsync(It.Is<AppClaim>(x => x.Type == "test"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdClaim);


        // Act
        var result = await ClaimTools.CreateClaimAsync(dto, _mockClaimClient.Object, _mapper, _mockValidator.Object, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Error.Should().BeNull();
        Assert.Contains(outgoingDto.Id.ToString(), result.Value.Id.ToString());
    }

    [Fact]
    public async Task GetClaimsAsync_ReturnsWarning_WhenClaimsIsNull()
    {
        // Arrange
        ClaimsQuery claimsQuery = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        ClaimsQueryIncomingDto dto = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        ICollection<ValidationResult> validationResults = [];
        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(true);
        _mockClaimClient.Setup(c => c.LoadClaimsAsync(claimsQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<AppClaim>)null!);


        // Act
        var result = await ClaimTools.GetClaimsAsync(dto, _mockClaimClient.Object, _mapper, _mockValidator.Object, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Warning");
        Assert.Contains("No claims found", result.Error.Errors?.First());
    }

    [Fact]
    public async Task GetClaimsAsync_HandlesNullMapperResult_Gracefully()
    {
        // Arrange
        ClaimsQuery claimsQuery = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        ClaimsQueryIncomingDto dto = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        var claims = new List<AppClaim> { new() { Id = Guid.NewGuid(), Type = "test" } };
        _mockClaimClient.Setup(c => c.LoadClaimsAsync(claimsQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        var localMockMapper = new Mock<IMapper>();
        localMockMapper.Setup(m => m.Map<ClaimsQuery>(It.IsAny<object>()))
            .Returns(claimsQuery);
        localMockMapper.Setup(m => m.Map<IEnumerable<ClaimOutgoingDto>>(claims))
            .Returns((IEnumerable<ClaimOutgoingDto>)null!);

        // Act
        var result = await ClaimTools.GetClaimsAsync(
            dto,
            _mockClaimClient.Object,
            localMockMapper.Object,
            _mockValidator.Object,
            _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Error");
    }

    [Fact]
    public async Task GetClaimDetailsAsync_ReturnsClaimDetails_OnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var claim = new AppClaim { Id = id, Type = "test" };
        var dto = new ClaimOutgoingDto { Id = id, Type = "test" };

        _mockClaimClient.Setup(c => c.LoadClaimDetailsAsync(id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(claim);


        // Act
        var result = await ClaimTools.GetClaimDetailsAsync(id.ToString(), _mockClaimClient.Object, _mapper, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetClaimDetailsAsync_ReturnsValidationFailed_WhenClaimIdIsInvalid()
    {
        // Arrange
        var invalidId = "not-a-guid";

        // Act
        var result = await ClaimTools.GetClaimDetailsAsync(invalidId, _mockClaimClient.Object, _mapper, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Validation Failed");
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
            _mapper,
            _mockLoggerFactory.Object);
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Validation Failed");
        Assert.Contains(expectedErrorFragment, result.Error.Errors?.First());
    }

    [Fact]
    public async Task GetClaimDetailsAsync_ReturnsValidationFailed_WhenClaimIdIsEmpty()
    {
        // Arrange
        var emptyId = "";

        // Act
        var result = await ClaimTools.GetClaimDetailsAsync(emptyId, _mockClaimClient.Object, _mapper, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Validation Failed");
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
        var result = await ClaimTools.GetClaimDetailsAsync(id.ToString(), _mockClaimClient.Object, _mapper, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Warning");
        Assert.Contains("Claim not found", result.Error.Errors?.First());
    }

    [Fact]
    public async Task GetClaimDetailsAsync_ReturnsError_OnException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockClaimClient.Setup(c => c.LoadClaimDetailsAsync(id.ToString(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await ClaimTools.GetClaimDetailsAsync(id.ToString(), _mockClaimClient.Object, _mapper, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Error");
        result.Error.Status.Should().Contain("Error");
        result.Error.Errors.Should().Contain(e => e.Contains("Database error"));
    }

    [Fact]
    public async Task DeleteClaimAsync_ReturnsDeletedClaim_OnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var deletedClaim = new AppClaim { Id = id, Type = "test" };
        var dto = new ClaimOutgoingDto { Id = id, Type = "test" };

        _mockClaimClient.Setup(c => c.DeleteClaimAsync(id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedClaim);


        // Act
        var result = await ClaimTools.DeleteClaimAsync(id.ToString(), _mockClaimClient.Object, _mapper, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Id.Should().Be(id);
    }

    [Fact]
    public async Task DeleteClaimAsync_ReturnsValidationFailed_WhenClaimIdIsInvalid()
    {
        // Arrange
        var invalidId = "invalid-guid";

        // Act
        var result = await ClaimTools.DeleteClaimAsync(invalidId, _mockClaimClient.Object, _mapper, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Validation Failed");
        Assert.Contains("must be a valid GUID", result.Error.Errors?.First());
    }

    [Fact]
    public async Task DeleteClaimAsync_ReturnsValidationFailed_WhenClaimIdIsEmpty()
    {
        // Arrange
        var emptyId = "";

        // Act
        var result = await ClaimTools.DeleteClaimAsync(emptyId, _mockClaimClient.Object, _mapper, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Validation Failed");
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
        var result = await ClaimTools.DeleteClaimAsync(id.ToString(), _mockClaimClient.Object, _mapper, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Error");
        result.Error.Status.Should().Contain("Error");
        result.Error.Errors.Should().Contain(e => e.Contains("Delete failed"));
    }

    [Fact]
    public async Task UpdateClaimAsync_ReturnsUpdatedClaim_OnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ClaimUpdateDto { Id = id, Type = "updated", Value = "test", ValueType = "string", DisplayText = "Test" };
        var claim = new AppClaim { Id = id, Type = "updated" };
        var updatedClaim = new AppClaim { Id = id, Type = "updated" };
        var outgoingDto = new ClaimOutgoingDto { Id = id, Type = "updated" };
        ICollection<ValidationResult> validationResults = [];

        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(true);

        _mockClaimClient.Setup(c => c.UpdateClaimAsync(id.ToString(), It.Is<AppClaim>(x => x.Id == id && x.Type == "updated"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedClaim);


        // Act
        var result = await ClaimTools.UpdateClaimAsync(id.ToString(), dto, _mockClaimClient.Object, _mapper, _mockValidator.Object, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Value.Id.Should().Be(id);
    }

    [Fact]
    public async Task UpdateClaimAsync_ReturnsValidationFailed_WhenValidatorFails()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ClaimUpdateDto { Id = id };
        ICollection<ValidationResult> validationResults = [new ValidationResult("Type is required")];

        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(false);

        // Act
        var result = await ClaimTools.UpdateClaimAsync(id.ToString(), dto, _mockClaimClient.Object, _mapper, _mockValidator.Object, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Validation Failed");
        Assert.Contains("Type is required", result.Error.Errors?.First());
    }

    [Fact]
    public async Task UpdateClaimAsync_ReturnsValidationFailed_WhenClaimIdIsInvalid()
    {
        // Arrange
        var invalidId = "not-a-guid";
        var dto = new ClaimUpdateDto { Id = Guid.NewGuid() };

        // Act
        var result = await ClaimTools.UpdateClaimAsync(invalidId, dto, _mockClaimClient.Object, _mapper, _mockValidator.Object, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Validation Failed");
        Assert.Contains("must be a valid GUID", result.Error.Errors?.First());
    }

    [Fact]
    public async Task UpdateClaimAsync_ReturnsValidationFailed_WhenIdMismatch()
    {
        // Arrange
        var pathId = Guid.NewGuid();
        var bodyId = Guid.NewGuid();
        var dto = new ClaimUpdateDto { Id = bodyId, Type = "test", Value = "test", ValueType = "string", DisplayText = "Test" };

        // Act
        var result = await ClaimTools.UpdateClaimAsync(pathId.ToString(), dto, _mockClaimClient.Object, _mapper, _mockValidator.Object, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Validation Failed");
        Assert.Contains("must match", result.Error.Errors?.First());
    }

    [Fact]
    public async Task UpdateClaimAsync_ReturnsError_OnException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ClaimUpdateDto { Id = id, Type = "test", Value = "test", ValueType = "string", DisplayText = "Test" };
        var claim = new AppClaim { Id = id, Type = "test" };
        ICollection<ValidationResult> validationResults = [];

        _mockValidator.Setup(v => v.TryValidate(dto, out validationResults))
            .Returns(true);

        _mockClaimClient.Setup(c => c.UpdateClaimAsync(id.ToString(), It.Is<AppClaim>(x => x.Id == id && x.Type == "test"), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await ClaimTools.UpdateClaimAsync(id.ToString(), dto, _mockClaimClient.Object, _mapper, _mockValidator.Object, _mockLoggerFactory.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error.Status.Should().Contain("Error");
        result.Error.Status.Should().Contain("Error");
        result.Error.Errors.Should().Contain(e => e.Contains("Update failed"));
    }
}
