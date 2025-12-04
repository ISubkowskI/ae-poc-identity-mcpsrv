namespace Ae.Poc.Identity.Mcp.Settings
{
    public sealed class HealthOptions
    {
        public const string Health = "Health"; // Configuration section name

        public bool Enabled { get; set; } = true;

        public int? Port { get; set; }



        public string LivePath { get; set; } = "/health/live";

        public string ReadyPath { get; set; } = "/health/ready";
    }
}
