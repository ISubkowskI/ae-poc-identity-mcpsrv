using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Settings;
using Ae.Poc.Identity.Mcp.Data;
using Flurl;

namespace Ae.Poc.Identity.Mcp.Services;

/// <summary>
/// Client for interacting with the identity storage API for claim management.
/// </summary>
public sealed class ClaimClient : IClaimClient
{
    private readonly ILogger<ClaimClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly IdentityStorageApiOptions _apiOptions;

    /// <summary>
    /// Base API endpoint for claim-related operations.
    /// </summary>
    private const string ClaimsApiBaseEndpoint = "masterdata/claims";

    /// <summary>
    /// Initializes a new instance of the <see cref="ClaimClient"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging.</param>
    /// <param name="identityStorageApiOptions">The options for configuring the identity storage API.</param>
    /// <param name="httpClient">The HTTP client for making API requests.</param>
    /// <param name="mapper">The AutoMapper instance for DTO mapping.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
    public ClaimClient(
        ILogger<ClaimClient> logger,
        IOptions<IdentityStorageApiOptions> identityStorageApiOptions,
        HttpClient httpClient,
        IMapper mapper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _apiOptions = identityStorageApiOptions?.Value ?? throw new ArgumentNullException(nameof(identityStorageApiOptions));

        _httpClient.BaseAddress = new Uri(_apiOptions.ApiUrl);
    }

    /// <summary>
    /// Parses the HTTP response message and maps it to an <see cref="AppClaim"/>.
    /// </summary>
    /// <param name="response">The HTTP response message from the API.</param>
    /// <param name="operationDescription">A description of the operation being performed (e.g., "creating new claim").</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An <see cref="AppClaim"/> instance mapped from the API response.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the API returns a success status code but the response content is null or cannot be deserialized.</exception>
    /// <exception cref="HttpRequestException">Thrown if the API call was not successful.</exception>
    private async Task<AppClaim> ParseAppClaimResponseAsync(HttpResponseMessage response, string operationDescription, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            var dto = await response.Content.ReadFromJsonAsync<ClaimDto>(cancellationToken: ct).ConfigureAwait(false);
            if (dto == null)
            {
                // This indicates an API issue if a 2xx response has a null body when AppClaimDto is expected.
                throw new InvalidOperationException($"API returned success for {operationDescription} but the response content was null or could not be deserialized to {nameof(ClaimDto)}.");
            }
            return _mapper.Map<AppClaim>(dto);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            throw new HttpRequestException($"Error {operationDescription}: {response.StatusCode} ({(int)response.StatusCode}). Response: {errorContent}", null, response.StatusCode);
        }
    }

    public async Task<ClaimsInfo?> GetClaimsInfoAsync(CancellationToken ct = default)
    {
        // This method is not implemented yet.
        return await Task.FromResult(new ClaimsInfo()
        {
            TotalCount = 11
        });
    }

    /// <summary>
    /// Asynchronously loads all claims from the identity storage API.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of <see cref="AppClaim"/>.</returns>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    public async Task<IEnumerable<AppClaim>?> LoadClaimsAsync(ClaimsQuery claimsQuery, CancellationToken ct = default)
    {
        string requestUri = Flurl.Url.Combine(_apiOptions.ApiBasePath, ClaimsApiBaseEndpoint)
            .SetQueryParams(new
            {
                skipped = claimsQuery.Skipped,
                numberOf = claimsQuery.NumberOf
            });

        _logger.LogInformation("Start {MethodName} for {RequestUri}...", nameof(LoadClaimsAsync), requestUri);

        try
        {
            var dtoList = await _httpClient.GetFromJsonAsync<IEnumerable<ClaimDto>>(requestUri: requestUri, cancellationToken: ct).ConfigureAwait(false);
            return _mapper.Map<IEnumerable<AppClaim>?>(dtoList);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Failed HTTP GET operation in {MethodName} for {RequestUri}", nameof(LoadClaimsAsync), requestUri);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error in {MethodName} for {RequestUri}", nameof(LoadClaimsAsync), requestUri);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously loads the details of a specific claim by its ID from the identity storage API.
    /// </summary>
    /// <param name="claimId">The unique identifier of the claim to load.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AppClaim"/> details.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the API returns a null DTO when a valid DTO was expected.</exception>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    public async Task<AppClaim?> LoadClaimDetailsAsync(string claimId, CancellationToken ct = default)
    {
        string requestUri = Flurl.Url.Combine(_apiOptions.ApiBasePath, ClaimsApiBaseEndpoint, claimId);
        _logger.LogInformation("Start {MethodName} for {RequestUri}...", nameof(LoadClaimDetailsAsync), requestUri);

        try
        {
            var res = await _httpClient.GetFromJsonAsync<ClaimDto>(requestUri: requestUri, cancellationToken: ct).ConfigureAwait(false);
            return _mapper.Map<AppClaim?>(res);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Failed HTTP GET operation in {MethodName} for {RequestUri}", nameof(LoadClaimDetailsAsync), requestUri);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error in {MethodName} for {RequestUri}", nameof(LoadClaimDetailsAsync), requestUri);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously deletes a claim by its ID from the identity storage API.
    /// </summary>
    /// <param name="claimId">The unique identifier of the claim to delete.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AppClaim"/> that was deleted.</returns>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    public async Task<AppClaim> DeleteClaimAsync(string claimId, CancellationToken ct = default)
    {
        string requestUri = Flurl.Url.Combine(_apiOptions.ApiBasePath, ClaimsApiBaseEndpoint, claimId);
        _logger.LogInformation("Start {MethodName} for {RequestUri}...", nameof(DeleteClaimAsync), requestUri);

        try
        {
            var httpResponse = await _httpClient.DeleteAsync(requestUri, ct).ConfigureAwait(false);
            return await ParseAppClaimResponseAsync(httpResponse, $"deleting claim '{claimId}'", ct);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Failed HTTP DELETE operation in {MethodName} for {RequestUri}", nameof(DeleteClaimAsync), requestUri);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error in {MethodName} for {RequestUri}", nameof(DeleteClaimAsync), requestUri);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously creates a new claim in the identity storage API.
    /// </summary>
    /// <param name="appClaim">The <see cref="AppClaim"/> object containing the data for the new claim.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="AppClaim"/>, including any server-generated values like ID.</returns>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    public async Task<AppClaim> CreateClaimAsync(AppClaim appClaim, CancellationToken ct = default)
    {
        string requestUri = Flurl.Url.Combine(_apiOptions.ApiBasePath, ClaimsApiBaseEndpoint);
        _logger.LogInformation("Start {MethodName} for {RequestUri}...", nameof(CreateClaimAsync), requestUri);

        try
        {
            var requestData = _mapper.Map<ClaimDto>(appClaim);
            var httpResponse = await _httpClient.PostAsJsonAsync(requestUri, requestData, ct).ConfigureAwait(false);
            return await ParseAppClaimResponseAsync(httpResponse, "creating new claim", ct);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Failed HTTP POST operation in {MethodName} for {RequestUri}", nameof(CreateClaimAsync), requestUri);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error in {MethodName} for {RequestUri}", nameof(CreateClaimAsync), requestUri);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously updates an existing claim by its ID in the identity storage API.
    /// </summary>
    /// <param name="claimId">The unique identifier of the claim to update.</param>
    /// <param name="appClaim">The <see cref="AppClaim"/> object containing the updated data for the claim.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="AppClaim"/>.</returns>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    public async Task<AppClaim> UpdateClaimAsync(string claimId, AppClaim appClaim, CancellationToken ct = default)
    {
        string requestUri = Flurl.Url.Combine(_apiOptions.ApiBasePath, ClaimsApiBaseEndpoint, claimId);
        _logger.LogInformation("Start {MethodName} for {RequestUri}...", nameof(UpdateClaimAsync), requestUri);

        try
        {
            var requestData = _mapper.Map<ClaimDto>(appClaim);
            var httpResponse = await _httpClient.PatchAsJsonAsync(requestUri, requestData, ct).ConfigureAwait(false);
            return await ParseAppClaimResponseAsync(httpResponse, $"updating claim '{claimId}'", ct);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Failed HTTP PATCH operation in {MethodName} for {RequestUri}", nameof(UpdateClaimAsync), requestUri);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error in {MethodName} for {RequestUri}", nameof(UpdateClaimAsync), requestUri);
            throw;
        }
    }


}
