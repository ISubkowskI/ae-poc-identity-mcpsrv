using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Services;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Ae.Poc.Identity.Mcp.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// This factory configures the test server with mocked dependencies.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IClaimClient> MockClaimClient { get; } = new Mock<IClaimClient>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing IClaimClient registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IClaimClient));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove the HttpClient registration for IClaimClient
            var httpClientDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IHttpClientFactory));
            // Note: We can't easily remove HttpClient factory, so we override IClaimClient instead

            // Add mocked IClaimClient
            services.AddScoped<IClaimClient>(_ => MockClaimClient.Object);
        });

        builder.UseEnvironment("Testing");
    }

    public void SetupMockClaimClient()
    {
        // Setup default mock behaviors
        var testClaims = new List<AppClaim>
        {
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Type = "email", Value = "test@example.com", ValueType = "string", DisplayText = "Email" },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Type = "role", Value = "admin", ValueType = "string", DisplayText = "Role" }
        };

        MockClaimClient.Setup(c => c.LoadClaimsAsync(It.IsAny<ClaimsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testClaims);

        MockClaimClient.Setup(c => c.LoadClaimDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken ct) =>
            {
                if (Guid.TryParse(id, out var guid))
                {
                    return testClaims.FirstOrDefault(c => c.Id == guid);
                }
                return null;
            });

        MockClaimClient.Setup(c => c.CreateClaimAsync(It.IsAny<AppClaim>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppClaim claim, CancellationToken ct) =>
            {
                claim.Id = Guid.NewGuid();
                return claim;
            });

        MockClaimClient.Setup(c => c.UpdateClaimAsync(It.IsAny<string>(), It.IsAny<AppClaim>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, AppClaim claim, CancellationToken ct) => claim);

        MockClaimClient.Setup(c => c.DeleteClaimAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken ct) =>
            {
                if (Guid.TryParse(id, out var guid))
                {
                    return testClaims.FirstOrDefault(c => c.Id == guid) ?? new AppClaim { Id = guid, Type = "deleted" };
                }
                return new AppClaim { Id = Guid.Empty, Type = "deleted" };
            });
    }
}
