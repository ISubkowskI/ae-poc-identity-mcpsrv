namespace Ae.Poc.Identity.Mcp.Settings;

public sealed class IdentityStorageApiOptions
{
    public const string IdentityStorageApi = "IdentityStorageApi";

    /// <summary>
    /// z.B. "http://localhost:5005"
    /// </summary>
    public string ApiUrl { get; set; } = String.Empty;

    /// <summary>
    /// z.B. "/api/v1"
    /// </summary>
    public string ApiBasePath { get; set; } = String.Empty;
}
