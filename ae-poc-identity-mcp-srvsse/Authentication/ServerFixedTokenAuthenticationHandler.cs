using Ae.Poc.Identity.Mcp.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Ae.Poc.Identity.Mcp.Authentication;

public sealed class ServerFixedTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string McpClientIdentifier = "mcp-client";
    private readonly ServerAuthenticationOptions _srvAuthOptions;

    public ServerFixedTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<ServerAuthenticationOptions> serverAuthenticationOptions)
        : base(options, logger, encoder)
    {
        _srvAuthOptions = serverAuthenticationOptions?.Value ?? throw new ArgumentNullException(nameof(serverAuthenticationOptions));
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var authorizationHeader = AuthenticationHeaderValue.Parse(authorizationHeaderValues.ToString());
        var configuredScheme = _srvAuthOptions.Scheme;

        if (authorizationHeader == null
            || string.IsNullOrWhiteSpace(configuredScheme)
            || !configuredScheme.Equals(authorizationHeader.Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var expectedToken = _srvAuthOptions.ExpectedToken;
        if (string.IsNullOrEmpty(expectedToken))
        {
            Logger.LogError("Authentication:ExpectedToken is not configured.");
            return Task.FromResult(AuthenticateResult.Fail("Server configuration error for authentication."));
        }

        if (authorizationHeader.Parameter == expectedToken ||
            (_srvAuthOptions.AllowWildcardToken && expectedToken == ServerAuthenticationOptions.WildcardToken))
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, McpClientIdentifier) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("Invalid token."));
    }
}
