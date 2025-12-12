using System.Reflection;
using Serilog;

namespace Ae.Poc.Identity.Mcp.Extensions;

public static class ConfigurationHelper
{
    public static string ResolveConfigDirectory(string[] args, string defaultPath)
    {
        string? configDir = null;
        try
        {
            configDir = args?.FirstOrDefault(a => a.StartsWith("--configpath=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1];
        }
        catch { /* ignore parse errors */ }

        configDir ??= Environment.GetEnvironmentVariable("CONFIG_PATH");

        // Resolve relative path to absolute
        if (!string.IsNullOrWhiteSpace(configDir))
        {
            if (!Path.IsPathRooted(configDir))
            {
                var absolutePath = Path.GetFullPath(configDir); // Resolutions relative to CWD
                if (Directory.Exists(absolutePath))
                {
                    return absolutePath;
                }
                else
                {
                    // Fallback: Try resolving relative to executable location
                    var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    if (!string.IsNullOrEmpty(exeDir))
                    {
                        var absolutePathFromExe = Path.GetFullPath(Path.Combine(exeDir, configDir));
                        if (Directory.Exists(absolutePathFromExe))
                        {
                            Log.Verbose("Resolved relative config path from exe location to: '{ConfigDir}'", absolutePathFromExe);
                            return absolutePathFromExe;
                        }
                    }
                }
            }
            return configDir;
        }

        Log.Verbose("No external config directory provided via --configpath or CONFIG_PATH, using default");
        return defaultPath;
    }
}
