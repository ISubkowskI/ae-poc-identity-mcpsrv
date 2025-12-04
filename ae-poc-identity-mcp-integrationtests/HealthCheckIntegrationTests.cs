using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using Xunit;

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
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
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
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Unhealthy");
    }

    [Fact]
    public async Task GetHealth_ReturnsNotFound_WhenDisabled()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("App:DisableHealthChecks", "true");
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
