namespace Ae.Poc.Identity.Mcp.Settings
{
    public sealed class ServerAuthenticationOptions
    {
        public const string Authentication = "Authentication"; // Configuration section name

        public const string WildcardToken = "*";

        public string Scheme { get; set; } = "Bearer";

        public string ExpectedToken { get; set; } = WildcardToken;

        // Allow wildcard for testing if needed
        public bool AllowWildcardToken { get; set; } = false;
    }
}