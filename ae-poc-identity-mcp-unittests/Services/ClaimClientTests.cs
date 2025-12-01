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
using System.Security.Claims;
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
        ClaimsQuery claimsQuery = new ()
        {
            Skipped = 0,
            NumberOf = 10
        };
        var dtos = new List<ClaimDto> { new() { Id = Guid.NewGuid(), Type = "test" } };
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

        _mockMapper.Setup(m => m.Map<IEnumerable<AppClaim>>(It.IsAny<IEnumerable<ClaimDto>>()))
            .Returns(claims);

        // Act
        var result = await _claimClient.LoadClaimsAsync(claimsQuery);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(claims[0].Id, result.First().Id);
    }

    [Fact]
    public async Task LoadClaimsAsync_ReturnsNull_OnNullResponse()
    {
        // Arrange
        ClaimsQuery claimsQuery = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create<IEnumerable<ClaimDto>>(null)
            });

        _mockMapper.Setup(m => m.Map<IEnumerable<AppClaim>>(It.IsAny<IEnumerable<ClaimDto>>()))
            .Returns((IEnumerable<AppClaim>)null);

        // Act
        var result = await _claimClient.LoadClaimsAsync(claimsQuery);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadClaimDetailsAsync_ReturnsClaim_OnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ClaimDto { Id = id, Type = "test" };
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

        _mockMapper.Setup(m => m.Map<AppClaim>(It.IsAny<ClaimDto>()))
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
        var dto = new ClaimDto { Type = "test" };
        var createdDto = new ClaimDto { Id = Guid.NewGuid(), Type = "test" };
        var createdClaim = new AppClaim { Id = createdDto.Id, Type = "test" };

        _mockMapper.Setup(m => m.Map<ClaimDto>(claim)).Returns(dto);
        _mockMapper.Setup(m => m.Map<AppClaim>(It.IsAny<ClaimDto>())).Returns(createdClaim);

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

    [Fact]
    public async Task UpdateClaimAsync_ReturnsUpdatedClaim_OnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var claim = new AppClaim { Id = id, Type = "updated" };
        var dto = new ClaimDto { Id = id, Type = "updated" };
        var updatedDto = new ClaimDto { Id = id, Type = "updated" };
        var updatedClaim = new AppClaim { Id = id, Type = "updated" };

        _mockMapper.Setup(m => m.Map<ClaimDto>(claim)).Returns(dto);
        _mockMapper.Setup(m => m.Map<AppClaim>(It.IsAny<ClaimDto>())).Returns(updatedClaim);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Patch && r.RequestUri.ToString().Contains(id.ToString())),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(updatedDto)
            });

        // Act
        var result = await _claimClient.UpdateClaimAsync(id.ToString(), claim);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task DeleteClaimAsync_ReturnsDeletedClaim_OnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var deletedDto = new ClaimDto { Id = id, Type = "deleted" };
        var deletedClaim = new AppClaim { Id = id, Type = "deleted" };

        _mockMapper.Setup(m => m.Map<AppClaim>(It.IsAny<ClaimDto>())).Returns(deletedClaim);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Delete && r.RequestUri.ToString().Contains(id.ToString())),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(deletedDto)
            });

        // Act
        var result = await _claimClient.DeleteClaimAsync(id.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task LoadClaimsAsync_ThrowsHttpRequestException_OnHttpError()
    {
        // Arrange
        ClaimsQuery claimsQuery = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Server error")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () => await _claimClient.LoadClaimsAsync(claimsQuery));
    }

    [Fact]
    public async Task LoadClaimDetailsAsync_ReturnNull_OnNullResponse()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains(id.ToString())),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create<ClaimDto>(null)
            });

        // Act 
        var result = await _claimClient.LoadClaimDetailsAsync(id.ToString());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateClaimAsync_ThrowsHttpRequestException_On404()
    {
        // Arrange
        var claim = new AppClaim { Type = "test" };
        var dto = new ClaimDto { Type = "test" };

        _mockMapper.Setup(m => m.Map<ClaimDto>(claim)).Returns(dto);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Not found")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () => await _claimClient.CreateClaimAsync(claim));
    }

    [Fact]
    public async Task LoadClaimsAsync_ThrowsTaskCanceledException_WhenCancelled()
    {
        // Arrange
        ClaimsQuery claimsQuery = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        var cts = new CancellationTokenSource();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Ok_Ok")
            });


        cts.Cancel(); // Cancel immediately

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await _claimClient.LoadClaimsAsync(claimsQuery, cts.Token));
    }

    [Fact]
    public async Task LoadClaimsAsync_ThrowsHttpRequestException_On401Unauthorized()
    {
        // Arrange
        ClaimsQuery claimsQuery = new()
        {
            Skipped = 0,
            NumberOf = 10
        };
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Unauthorized access")
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _claimClient.LoadClaimsAsync(claimsQuery));

        Assert.Contains("401", exception.Message);
    }

}
