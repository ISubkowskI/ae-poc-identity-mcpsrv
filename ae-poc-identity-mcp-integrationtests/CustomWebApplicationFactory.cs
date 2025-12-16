using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Ae.Poc.Identity.Mcp.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// This factory configures the test server with mocked dependencies.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public CustomWebApplicationFactory()
    {
        Console.WriteLine("CustomWebApplicationFactory constructor called");
    }

    public Mock<IClaimClient> MockClaimClient { get; } = new Mock<IClaimClient>();
    public Mock<IClaimClientHealth> MockClaimClientHealth { get; } = new Mock<IClaimClientHealth>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing IClaimClient registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IClaimClient));
            if (descriptor != null) services.Remove(descriptor);

            var descriptorHealth = services.SingleOrDefault(d => d.ServiceType == typeof(IClaimClientHealth));
            if (descriptorHealth != null) services.Remove(descriptorHealth);

            // Add mocked IClaimClient and IClaimClientHealth
            services.AddScoped<IClaimClient>(_ => MockClaimClient.Object);
            services.AddScoped<IClaimClientHealth>(_ => MockClaimClientHealth.Object);
        });

        builder.UseEnvironment("Testing");
        builder.UseSetting("App:Url", "http://localhost"); // Match Default TestServer functionality
        builder.UseSetting("Authentication:ExpectedToken", "YOUR_SUPER_SECRET_AND_UNIQUE_TOKEN_REPLACE_ME");
    }

    public void SetupMockClaimClient()
    {
        MockClaimClient.Reset();
        MockClaimClientHealth.Reset();

        // Setup default mock behaviors
        var testClaims = new List<AppClaim>
        {
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Type = "email", Value = "test@example.com", ValueType = "string", DisplayText = "Email" },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Type = "role", Value = "admin", ValueType = "string", DisplayText = "Role" }
        };

        MockClaimClient.Setup(c => c.LoadClaimsAsync(It.IsAny<ClaimsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testClaims);

        MockClaimClient.Setup(c => c.GetClaimsInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimsInfo { TotalCount = testClaims.Count });
        
        MockClaimClientHealth.Setup(c => c.GetHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ae.Poc.Identity.Mcp.Dtos.DependencyHealthDto { IsReady = true, Version = "1.0.0", ClientId = "test-client" });

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
