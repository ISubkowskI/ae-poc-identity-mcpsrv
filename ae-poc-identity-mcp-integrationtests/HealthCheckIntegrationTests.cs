using Ae.Poc.Identity.Mcp.Data;
using FluentAssertions;
using Moq;
using System.Net;

namespace Ae.Poc.Identity.Mcp.IntegrationTests;

public class HealthCheckIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public HealthCheckIntegrationTests(CustomWebApplicationFactory factory)
    {
        try
        {
            _factory = factory;
            _factory.SetupMockClaimClient(); // Reset mocks
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in constructor: {ex}");
            throw;
        }
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthy_WhenBackendIsReachable()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Mock backend to return success
        _factory.MockClaimClient
            .Setup(c => c.GetClaimsInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimsInfo { TotalCount = 10 });

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"status\":\"Healthy\"");
    }

    [Fact]
    public async Task GetHealth_ReturnsUnhealthy_WhenBackendIsUnreachable()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Mock backend to throw exception
        _factory.MockClaimClient
            .Setup(c => c.GetClaimsInfoAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Backend down"));

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"status\":\"Unhealthy\"");
    }

    [Fact]
    public async Task GetHealth_ReturnsJsonStructure_WhenHealthy()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Mock backend to return success
        _factory.MockClaimClient
            .Setup(c => c.GetClaimsInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimsInfo { TotalCount = 10 });

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        // Simple string checks for JSON structure to avoid adding System.Text.Json dependency if not present,
        // but robust enough to verify the UIResponseWriter is working.
        content.Should().Contain("\"status\":\"Healthy\"");
        content.Should().Contain("\"entries\":");
        content.Should().Contain("\"claim-api\":");
        content.Should().Contain("\"self\":");
    }

    [Fact]
    public async Task GetHealth_ReturnsNotFound_WhenDisabled()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Health:Enabled", "false");
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
