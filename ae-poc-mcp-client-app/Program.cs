using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Authentication;
using ModelContextProtocol.Client;

var builder = Host.CreateApplicationBuilder(args);

//builder.Configuration
//    .AddEnvironmentVariables()
//    .AddUserSecrets<Program>();

var httpClientTransport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri(builder.Configuration["McpServer:Endpoint"] ?? "http://localhost:3001/identity/mcp/sse"),
    TransportMode = HttpTransportMode.AutoDetect,
    AdditionalHeaders = new Dictionary<string, string>
    {
        { "Authorization", "Bearer YOUR_SUPER_SECRET_AND_UNIQUE_TOKEN_REPLACE_ME" }
    }
});

var stdioClientTransport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "Everything",
    Command = "npx",
    Arguments = ["-y", "@modelcontextprotocol/server-everything"],
});

var client = await McpClient.CreateAsync(stdioClientTransport);