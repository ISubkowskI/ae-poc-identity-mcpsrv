using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Ae.Poc.Identity.Mcp.IntegrationTests.Tools;

/// <summary>
/// Integration tests for ClaimTools MCP server tools.
/// These tests verify end-to-end functionality with the test server.
/// </summary>
public class ClaimToolsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly IServiceScope _scope;
    private readonly IClaimTools _claimTools;

    public ClaimToolsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.SetupMockClaimClient();

        // Create a scope to get services
        _scope = _factory.Services.CreateScope();
        
        // Ensure ClaimTools is registered and resolvable
        // If it's not registered in the test factory's services (e.g. if Program.cs registration isn't effective here),
        // we might need to manually instantiate it, but ideally it should be in DI.
        // Assuming Program.cs registration works.
        _claimTools = _scope.ServiceProvider.GetRequiredService<IClaimTools>();
    }

    [Fact]
    public async Task GetClaimsAsync_ReturnsNumberOfClaims_Integration()
    {
        // Arrange
        ClaimsQueryIncomingDto dto = new ()
        {
            Skipped = 0,
            NumberOf = 10
        };

        // Act
        var result = await _claimTools.GetClaimsAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Claims.Count());
        Assert.Contains(result.Value.Claims, c => c.Type == "email");
        Assert.Contains(result.Value.Claims, c => c.Type == "role");
        Assert.Contains(result.Value.Claims, c => c.Type == "role");
    }

    [Fact]
    public async Task GetClaimsAsync_ReturnsClaimsInfo_WhenRequested_Integration()
    {
        // Arrange
        ClaimsQueryIncomingDto dto = new()
        {
            Skipped = 0,
            NumberOf = 10,
            WithClaimsInfo = true
        };

        // Act
        var result = await _claimTools.GetClaimsAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.ClaimsInfo);
        Assert.Equal(2, result.Value.ClaimsInfo!.TotalCount);
    }

    [Fact]
    public async Task GetClaimDetailsAsync_ReturnsClaimDetails_WhenClaimExists_Integration()
    {
        // Arrange
        var claimId = "00000000-0000-0000-0000-000000000001";

        // Act
        var result = await _claimTools.GetClaimDetailsAsync(claimId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("email", result.Value.Type);
        Assert.Equal("test@example.com", result.Value.Value);
    }

    [Fact]
    public async Task GetClaimDetailsAsync_ReturnsWarning_WhenClaimNotFound_Integration()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _claimTools.GetClaimDetailsAsync(nonExistentId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Claim not found", result.Error.Errors?.First());
    }

    [Fact]
    public async Task CreateClaimAsync_CreatesNewClaim_Integration()
    {
        // Arrange
        var createDto = new ClaimCreateDto
        {
            Type = "newclaim",
            Value = "newvalue",
            ValueType = "string",
            DisplayText = "New Claim",
            Description = "Test claim"
        };

        // Act
        var result = await _claimTools.CreateClaimAsync(createDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("newclaim", result.Value.Type);
        Assert.Equal("newvalue", result.Value.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public async Task CreateClaimAsync_ReturnsValidationFailed_WhenDtoIsInvalid_Integration()
    {
        // Arrange
        var invalidDto = new ClaimCreateDto
        {
            // Missing required fields
            Type = "",
            Value = "",
            ValueType = "",
            DisplayText = ""
        };

        // Act
        var result = await _claimTools.CreateClaimAsync(invalidDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("Validation Failed", result.Error.Status);
    }

    [Fact]
    public async Task UpdateClaimAsync_UpdatesExistingClaim_Integration()
    {
        // Arrange
        var claimId = Guid.NewGuid();
        var updateDto = new ClaimUpdateDto
        {
            Id = claimId,
            Type = "updated",
            Value = "updatedvalue",
            ValueType = "string",
            DisplayText = "Updated Claim"
        };

        // Act
        var result = await _claimTools.UpdateClaimAsync(claimId.ToString(), updateDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("updated", result.Value.Type);
        Assert.Equal("updatedvalue", result.Value.Value);
    }

    [Fact]
    public async Task UpdateClaimAsync_ReturnsValidationFailed_WhenIdMismatch_Integration()
    {
        // Arrange
        var pathId = Guid.NewGuid();
        var bodyId = Guid.NewGuid();
        var updateDto = new ClaimUpdateDto
        {
            Id = bodyId,
            Type = "test",
            Value = "test",
            ValueType = "string",
            DisplayText = "Test"
        };

        // Act
        var result = await _claimTools.UpdateClaimAsync(pathId.ToString(), updateDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("must match", result.Error.Errors?.First());
    }

    [Fact]
    public async Task DeleteClaimAsync_DeletesClaim_Integration()
    {
        // Arrange
        var claimId = "00000000-0000-0000-0000-000000000002";

        // Act
        var result = await _claimTools.DeleteClaimAsync(claimId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(Guid.Parse(claimId), result.Value.Id);
    }

    [Fact]
    public async Task DeleteClaimAsync_ReturnsValidationFailed_WhenIdIsInvalid_Integration()
    {
        // Arrange
        var invalidId = "not-a-guid";

        // Act
        var result = await _claimTools.DeleteClaimAsync(invalidId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Contains("must be a valid GUID", result.Error.Errors?.First());
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }
}
