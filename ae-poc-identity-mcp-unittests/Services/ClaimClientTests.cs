using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Services;
using Ae.Poc.Identity.Mcp.Settings;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Ae.Poc.Identity.Mcp.Unittests.Services;

public class ClaimClientTests
{
    private readonly Mock<ILogger<ClaimClient>> _mockLogger;
    private readonly Mock<IOptions<IdentityStorageApiOptions>> _mockOptions;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ClaimClient _claimClient;

    public ClaimClientTests()
    {
        _mockLogger = new Mock<ILogger<ClaimClient>>();
        _mockOptions = new Mock<IOptions<IdentityStorageApiOptions>>();
        _mockMapper = new Mock<IMapper>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _mockOptions.Setup(o => o.Value).Returns(new IdentityStorageApiOptions
        {
            ApiUrl = "http://localhost",
            ApiBasePath = "api"
        });

        _claimClient = new ClaimClient(
            _mockLogger.Object,
            _mockOptions.Object,
            _httpClient,
            _mockMapper.Object
        );
    }

    [Fact]
    public async Task LoadClaimsAsync_ReturnsClaims_OnSuccess()
    {
        // Arrange
        var dtos = new List<AppClaimDto> { new() { Id = Guid.NewGuid(), Type = "test" } };
        var claims = new List<AppClaim> { new() { Id = dtos[0].Id, Type = "test" } };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(dtos)
            });

        _mockMapper.Setup(m => m.Map<IEnumerable<AppClaim>>(It.IsAny<IEnumerable<AppClaimDto>>()))
            .Returns(claims);

        // Act
        var result = await _claimClient.LoadClaimsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(claims[0].Id, result.First().Id);
    }

    [Fact]
    public async Task LoadClaimsAsync_ReturnsEmpty_OnNullResponse()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create<IEnumerable<AppClaimDto>>(null)
            });

        _mockMapper.Setup(m => m.Map<IEnumerable<AppClaim>>(It.IsAny<IEnumerable<AppClaimDto>>()))
            .Returns((IEnumerable<AppClaim>)null);

        // Act
        var result = await _claimClient.LoadClaimsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadClaimDetailsAsync_ReturnsClaim_OnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new AppClaimDto { Id = id, Type = "test" };
        var claim = new AppClaim { Id = id, Type = "test" };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains(id.ToString())),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(dto)
            });

        _mockMapper.Setup(m => m.Map<AppClaim>(It.IsAny<AppClaimDto>()))
            .Returns(claim);

        // Act
        var result = await _claimClient.LoadClaimDetailsAsync(id.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task CreateClaimAsync_ReturnsCreatedClaim_OnSuccess()
    {
        // Arrange
        var claim = new AppClaim { Type = "test" };
        var dto = new AppClaimDto { Type = "test" };
        var createdDto = new AppClaimDto { Id = Guid.NewGuid(), Type = "test" };
        var createdClaim = new AppClaim { Id = createdDto.Id, Type = "test" };

        _mockMapper.Setup(m => m.Map<AppClaimDto>(claim)).Returns(dto);
        _mockMapper.Setup(m => m.Map<AppClaim>(It.IsAny<AppClaimDto>())).Returns(createdClaim);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = JsonContent.Create(createdDto)
            });

        // Act
        var result = await _claimClient.CreateClaimAsync(claim);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdClaim.Id, result.Id);
    }
}
