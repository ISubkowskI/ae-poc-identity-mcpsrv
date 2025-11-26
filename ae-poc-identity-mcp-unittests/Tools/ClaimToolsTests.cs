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
        ICollection<ValidationResult> validationResults = new List<ValidationResult> { new("Invalid") };
        
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
        ICollection<ValidationResult> validationResults = new List<ValidationResult>();

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
}
