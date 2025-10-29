using Ae.Poc.Identity.Mcp.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Ae.Poc.Identity.Mcp.Authentication;

public sealed class ServerFixedTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ServerAuthenticationOptions _srvAuthOptions;
    //private readonly IConfiguration _configuration;
    // private const string AuthenticationScheme = "Bearer"; // Scheme is read from options/config

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
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var authorizationHeader = AuthenticationHeaderValue.Parse(authorizationHeaderValues.ToString());
        var configuredScheme = _srvAuthOptions.Scheme;

        if (authorizationHeader == null || !configuredScheme.Equals(authorizationHeader.Scheme, System.StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var expectedToken = _srvAuthOptions.ExpectedToken;
        if (string.IsNullOrEmpty(expectedToken))
        {
            Logger.LogError("Authentication:ExpectedToken is not configured.");
            return Task.FromResult(AuthenticateResult.Fail("Server configuration error for authentication."));
        }

        if (expectedToken == "*" || authorizationHeader.Parameter == expectedToken) // Allow wildcard for testing if needed
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "mcp-client") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("Invalid token."));
    }
}

