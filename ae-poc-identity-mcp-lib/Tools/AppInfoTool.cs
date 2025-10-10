using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Settings;

namespace Ae.Poc.Identity.Mcp.Tools;

/// <summary>
/// Provides MCP server tools for retrieving application information.
/// This static class contains methods exposed through the MCP server interface
/// to provide details about the running application.
/// </summary>
[McpServerToolType]
public static class AppInfoTool
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = false };

    /// <summary>
    /// Returns the current application version and time information as a JSON object.
    /// </summary>
    [McpServerTool(Name = "general-get_app_version"), Description("Returns the current application version, local time, UTC time, and UTC ticks as a JSON object.")]
    public static string GetAppVersion(IOptions<AppOptions> appOptions)
    {
        string appVersion = appOptions?.Value?.Version ?? "?.?";

        var dt = DateTimeOffset.Now;
        var dto = new AppVersionDto
        {
            AppVersion = appVersion,
            AppNow = dt,
            AppNowUtc = dt.ToUniversalTime(),
            AppUtcTicks = dt.UtcTicks,
        };

        return JsonSerializer.Serialize(dto, JsonSerializerOptions);
    }
}
