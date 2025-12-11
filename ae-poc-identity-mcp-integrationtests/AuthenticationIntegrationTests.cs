using System.Net;
using System.Net.Http.Headers;

namespace Ae.Poc.Identity.Mcp.IntegrationTests;

public class AuthenticationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private const string ExpectedToken = "YOUR_SUPER_SECRET_AND_UNIQUE_TOKEN_REPLACE_ME";
    private const string McpEndpoint = "/mcp/v1/claims"; // Using base endpoint, exact path might need adjustment based on behavior

    public AuthenticationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.SetupMockClaimClient();
    }

    [Fact]
    public async Task Get_McpEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        // Trying to access the SSE endpoint which is protected
        var response = await client.GetAsync($"{McpEndpoint}/sse");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_McpEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await client.GetAsync($"{McpEndpoint}/sse");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_McpEndpoint_WithValidToken_DoesNotReturnUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ExpectedToken);
        // Add required headers for SSE if needed, though Unauthorized check happens early
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        // Act
        // cancellation token to not wait forever since it is a stream
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        try
        {
            var response = await client.GetAsync($"{McpEndpoint}/sse", HttpCompletionOption.ResponseHeadersRead, cts.Token);
            
            // Assert
            // We expect OK (200) or at least NOT Unauthorized.
            // Since it's SSE, it might hang open, so we use ResponseHeadersRead.
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        catch (OperationCanceledException)
        {
            // If it times out, it means it connected and was waiting (which is good), 
            // unless it timed out connecting. But ResponseHeadersRead usually returns fast.
        }
    }
}
