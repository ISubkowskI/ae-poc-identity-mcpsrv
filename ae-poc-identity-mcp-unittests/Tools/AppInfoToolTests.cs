using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Settings;
using Ae.Poc.Identity.Mcp.Tools;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Ae.Poc.Identity.Mcp.Unittests.Tools;

public class AppInfoToolTests
{
    [Fact]
    public async Task GetAppVersion_ReturnsCorrectInfo()
    {
        // Arrange
        var appOptions = new AppOptions { Version = "1.0.0" };
        var mockOptions = new Mock<IOptions<AppOptions>>();
        mockOptions.Setup(o => o.Value).Returns(appOptions);

        // Act
        var result = await AppInfoTool.GetAppVersion(mockOptions.Object);

        // Assert
        Assert.NotNull(result);
        var dto = Assert.IsType<AppVersionDto>(result);
        Assert.Equal("1.0.0", dto.AppVersion);
        Assert.True(dto.AppNow <= DateTimeOffset.Now);
        Assert.True(dto.AppNow > DateTimeOffset.Now.AddSeconds(-5)); // Sanity check for time
    }

    [Fact]
    public async Task GetAppVersion_ReturnsDefaultVersion_WhenOptionsNull()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<AppOptions>>();
        mockOptions.Setup(o => o.Value).Returns((AppOptions)null!);

        // Act
        var result = await AppInfoTool.GetAppVersion(mockOptions.Object);

        // Assert
        Assert.NotNull(result);
        var dto = Assert.IsType<AppVersionDto>(result);
        Assert.Equal("?.?", dto.AppVersion);
    }
}
