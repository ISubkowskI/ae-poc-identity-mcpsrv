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
        _factory.MockClaimClientHealth
            .Setup(c => c.GetHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ae.Poc.Identity.Mcp.Dtos.DependencyHealthDto { IsReady = true, Version = "1.0", ClientId = "test" });

        // Act
        var response = await client.GetAsync("http://localhost:9007/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"status\":\"Healthy\"");
        content.Should().Contain("identitystorage-api");
    }

    [Fact]
    public async Task GetHealth_ReturnsUnhealthy_WhenBackendIsUnreachable()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Mock backend to return not ready
        _factory.MockClaimClientHealth
            .Setup(c => c.GetHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ae.Poc.Identity.Mcp.Dtos.DependencyHealthDto { IsReady = false });

        // Act
        var response = await client.GetAsync("http://localhost:9007/health/ready");

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
        _factory.MockClaimClientHealth
            .Setup(c => c.GetHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ae.Poc.Identity.Mcp.Dtos.DependencyHealthDto { IsReady = true, Version = "1.0", ClientId = "test" });

        // Act
        var response = await client.GetAsync("http://localhost:9007/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        // Verify custom fields and structure
        content.Should().Contain("\"status\":\"Healthy\"");
        content.Should().Contain("\"results\":");
        content.Should().Contain("\"identitystorage-api\":");
        content.Should().Contain("\"self\":");
        content.Should().Contain("\"duration\":"); // Added duration check
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
        var response = await client.GetAsync("http://localhost:9007/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
