using Ae.Poc.Identity.Mcp.Extensions;
using Xunit;

namespace Ae.Poc.Identity.Mcp.UnitTests.Extensions;

public class ConfigurationHelperTests
{
    private readonly string _defaultPath;

    public ConfigurationHelperTests()
    {
        _defaultPath = Directory.GetCurrentDirectory();
        // Clear environment variable before each test to ensure isolation
        Environment.SetEnvironmentVariable("CONFIG_PATH", null);
    }

    [Fact]
    public void ResolveConfigDirectory_ReturnsFromArgs_WhenArgsProvided()
    {
        // Arrange
        // We need a path that actually exists because the helper checks Directory.Exists
        var args = new[] { $"--configpath={_defaultPath}" };

        // Act
        var result = ConfigurationHelper.ResolveConfigDirectory(args, "default");

        // Assert
        Assert.Equal(_defaultPath, result);
    }

    [Fact]
    public void ResolveConfigDirectory_ReturnsFromEnvVar_WhenArgsMissing()
    {
        // Arrange
        Environment.SetEnvironmentVariable("CONFIG_PATH", _defaultPath);
        var args = Array.Empty<string>();

        // Act
        var result = ConfigurationHelper.ResolveConfigDirectory(args, "default");

        // Assert
        Assert.Equal(_defaultPath, result);
    }

    [Fact]
    public void ResolveConfigDirectory_ReturnsDefault_WhenNoArgsOrEnvVar()
    {
        // Arrange
        var args = Array.Empty<string>();
        string defaultVal = _defaultPath;

        // Act
        var result = ConfigurationHelper.ResolveConfigDirectory(args, defaultVal);

        // Assert
        Assert.Equal(defaultVal, result);
    }

    [Fact]
    public void ResolveConfigDirectory_ResolvesRelativePath_ToAbsolute()
    {
        // Arrange
        // Use "." which is a valid relative path to Current Directory
        var args = new[] { "--configpath=." };

        // Act
        var result = ConfigurationHelper.ResolveConfigDirectory(args, "default");

        // Assert
        // Should resolve "." to CWD absolute path
        Assert.Equal(_defaultPath, result);
        Assert.True(Path.IsPathRooted(result));
    }

    [Fact]
    public void ResolveConfigDirectory_PrioritizesArgsOverEnvVar()
    {
        // Arrange
        Environment.SetEnvironmentVariable("CONFIG_PATH", "some/env/path");
        var args = new[] { $"--configpath={_defaultPath}" };

        // Act
        var result = ConfigurationHelper.ResolveConfigDirectory(args, "default");

        // Assert
        Assert.Equal(_defaultPath, result);
    }
}
