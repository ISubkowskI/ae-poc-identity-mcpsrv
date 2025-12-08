namespace Ae.Poc.Identity.Mcp.Settings
{
    public sealed class AppOptions
    {
        public const string App = "App"; // Configuration section name
        public const int DefaultPort = 8080;

        /// <summary>
        /// The name of the application. (e.g. "MCP Server")
        /// </summary>
        public string Name { get; set; } = "???";

        /// <summary>
        /// The version of the application. (e.g. "1.0.1")
        /// </summary>
        public string Version { get; set; } = "?.?";

        /// <summary>
        /// The base URL of the application.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        public string MapMcpPattern { get; set; } = string.Empty;

        public Uri? Uri
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Url))
                {
                    return null;
                }

                if (Uri.TryCreate(Url, UriKind.Absolute, out var uri))
                {
                    return uri;
                }

                return null;
            }
        }
    }
}