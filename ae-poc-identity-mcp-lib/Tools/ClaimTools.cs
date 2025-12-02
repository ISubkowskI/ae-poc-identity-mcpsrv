using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Services;
using AutoMapper;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ae.Poc.Identity.Mcp.Tools;

/// <summary>
/// Provides a collection of MCP server tools for managing and retrieving claims.
/// This class exposes claim-related functionality through the MCP server interface.
/// All methods return JSON-serialized responses and map entities to DTOs via AutoMapper.
/// </summary>
[McpServerToolType]
public class ClaimTools : IClaimTools
{
    private readonly IClaimClient _claimClient;
    private readonly IMapper _mapper;
    private readonly IDtoValidator _validator;
    private readonly ILogger<ClaimTools> _logger;

    public ClaimTools(
        IClaimClient claimClient,
        IMapper mapper,
        IDtoValidator validator,
        ILogger<ClaimTools> logger)
    {
        _claimClient = claimClient;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of claims from the claim client, maps them to DTOs, and serializes the result to JSON.
    /// </summary>
    [McpServerTool(Name = "identity-get_claims", UseStructuredContent = true)]
    [Description("Retrieve a list of claims.")]
    public async Task<ToolResult<ClaimsOutgoingDto, ErrorOutgoingDto>> GetClaimsAsync(
    [Description(@"Claim query parameters.
Expected JSON structure:
{
  ""skipped"": ""int"",
  ""numberOf"": ""int"",
}")] ClaimsQueryIncomingDto queryIncomingDto,
        CancellationToken ct = default)
    {
        try
        {
            if (!_validator.TryValidate(queryIncomingDto, out var validationResults))
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown validation error").ToArray();
                return ToolResultFactory.ValidationFailed<ClaimsOutgoingDto>(errors);
            }

            var claimsQuery = _mapper.Map<ClaimsQuery>(queryIncomingDto);
            if (claimsQuery == null)
            {
                _logger.LogError("Failed to map query incoming Dto to claims query");
                return ToolResultFactory.Failure<ClaimsOutgoingDto>(["Failed to map query incoming Dto to claims query"]);
            }

            _logger.LogInformation("Retrieving all claims");
            var claims = await _claimClient.LoadClaimsAsync(claimsQuery, ct);
            if (claims == null)
            {
                _logger.LogWarning("No claims found in the system");
                return ToolResultFactory.Warning<ClaimsOutgoingDto>("No claims found");
            }

            var res = _mapper.Map<IEnumerable<ClaimOutgoingDto>>(claims);
            if (res == null)
            {
                _logger.LogError("Failed to map claims to outgoing DTOs");
                return ToolResultFactory.Failure<ClaimsOutgoingDto>(["Failed to map claims to outgoing DTOs"]);
            }

            ClaimsOutgoingDto resDto = new ()
            {
                Claims = res,
                ClaimsInfo = (queryIncomingDto.WithClaimsInfo ?? false) ? _mapper.Map<ClaimsInfoOutgoingDto>(await _claimClient.GetClaimsInfoAsync(ct)) : null
            };

            _logger.LogInformation("Successfully retrieved {ClaimCount} claims", res.Count());
            return ToolResultFactory.Success(resDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving claims");
            return ToolResultFactory.FromException<ClaimsOutgoingDto>(ex);
        }
    }

    /// <summary>
    /// Retrieves the details of a specific claim by its ID, maps it to a DTO, and serializes the result to JSON.
    /// </summary>
    [McpServerTool(Name = "identity-get_claim_details", UseStructuredContent = true)]
    [Description("Retrieve details for a single claim by id.")]
    public async Task<ToolResult<ClaimOutgoingDto, ErrorOutgoingDto>> GetClaimDetailsAsync(
        [Description("The id of the claim to get details for")] string claimId,
        CancellationToken ct = default)
    {
        try
        {
            if (!TryParseClaimId(claimId, out _, out var errorMessage))
            {
                return ToolResultFactory.ValidationFailed<ClaimOutgoingDto>([errorMessage ?? "Unknown validation error"]);
            }

            var claim = await _claimClient.LoadClaimDetailsAsync(claimId, ct);
            if (claim is null)
            {
                return ToolResultFactory.Warning<ClaimOutgoingDto>("Claim not found");
            }

            var res = _mapper.Map<ClaimOutgoingDto>(claim);
            return ToolResultFactory.Success(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving claim details");
            return ToolResultFactory.FromException<ClaimOutgoingDto>(ex);
        }
    }

    /// <summary>
    /// Deletes a claim by its ID, maps the deleted claim to a DTO, and serializes the result to JSON.
    /// </summary>
    [McpServerTool(Name = "identity-delete_claim")]
    [Description("Delete a claim by id.")]
    public async Task<ToolResult<ClaimOutgoingDto, ErrorOutgoingDto>> DeleteClaimAsync(
        [Description("The id of the claim to delete")] string claimId,
        CancellationToken ct = default)
    {
        try
        {
            if (!TryParseClaimId(claimId, out _, out var errorMessage))
            {
                return ToolResultFactory.ValidationFailed<ClaimOutgoingDto>([errorMessage ?? "Unknown validation error"]);
            }

            var deletedClaim = await _claimClient.DeleteClaimAsync(claimId, ct);
            var res = _mapper.Map<ClaimOutgoingDto>(deletedClaim);
            return ToolResultFactory.Success(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting claim");
            return ToolResultFactory.FromException<ClaimOutgoingDto>(ex);
        }
    }

    /// <summary>
    /// Creates a new claim using the provided DTO, maps it to the entity, and serializes the created claim to JSON.
    /// </summary>
    [McpServerTool(Name = "identity-create_claim")]
    [Description("Create a new claim.")]
    public async Task<ToolResult<ClaimOutgoingDto, ErrorOutgoingDto>> CreateClaimAsync(
        [Description(@"The data for the new claim. The server will assign the 'Id'.
Expected JSON structure:
{
  ""Type"": ""string"",
  ""Value"": ""string"",
  ""ValueType"": ""string"",
  ""DisplayText"": ""string"",
  ""Properties"": { ""key1"": ""value1"", ... } (optional),
  ""Description"": ""string"" (optional, max 500 chars)
}")] ClaimCreateDto claimDto,
        CancellationToken ct = default)
    {
        try
        {
            if (!_validator.TryValidate(claimDto, out var validationResults))
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown validation error").ToArray();
                return ToolResultFactory.ValidationFailed<ClaimOutgoingDto>(errors);
            }

            var appClaim = _mapper.Map<AppClaim>(claimDto);
            var createdClaim = await _claimClient.CreateClaimAsync(appClaim, ct);
            var res = _mapper.Map<ClaimOutgoingDto>(createdClaim);
            return ToolResultFactory.Success(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating claim");
            return ToolResultFactory.FromException<ClaimOutgoingDto>(ex);
        }
    }

    /// <summary>
    /// Updates an existing claim by its ID using the provided DTO, maps it to the entity, and serializes the updated claim to JSON.
    /// </summary>
    [McpServerTool(Name = "identity-update_claim")]
    [Description("Update a claim by id.")]
    public async Task<ToolResult<ClaimOutgoingDto, ErrorOutgoingDto>> UpdateClaimAsync(
        [Description("The id of the claim to update")] string claimId,
        [Description(@"The data to update the claim. The 'Id' in the body must match the 'claimId' in the path.
Expected JSON structure:
{
  ""Id"": ""guid_string"",
  ""Type"": ""string"",
  ""Value"": ""string"",
  ""ValueType"": ""string"",
  ""DisplayText"": ""string"",
  ""Properties"": { ""key1"": ""value1"", ... } (optional),
  ""Description"": ""string"" (optional, max 500 chars)
}")] ClaimUpdateDto claimDto,
        CancellationToken ct = default)
    {
        try
        {
            if (!TryParseClaimId(claimId, out var parsedClaimId, out var errorMessage))
            {
                return ToolResultFactory.ValidationFailed<ClaimOutgoingDto>([errorMessage ?? "Unknown validation error"]);
            }

            // Compare claimId with claimDto.Id
            if (parsedClaimId != claimDto.Id)
            {
                return ToolResultFactory.ValidationFailed<ClaimOutgoingDto>(["The claimId path parameter must match the Id field in the request body."]);
            }

            if (!_validator.TryValidate(claimDto, out var validationResults))
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown validation error").ToArray();
                return ToolResultFactory.ValidationFailed<ClaimOutgoingDto>(errors);
            }

            var appClaim = _mapper.Map<AppClaim>(claimDto);
            var updatedClaim = await _claimClient.UpdateClaimAsync(claimId, appClaim, ct);
            var res = _mapper.Map<ClaimOutgoingDto>(updatedClaim);
            return ToolResultFactory.Success(res);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating claim");
            return ToolResultFactory.FromException<ClaimOutgoingDto>(ex);
        }
    }

    private static bool TryParseClaimId(string claimId, out Guid parsedId, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(claimId))
        {
            errorMessage = "The claimId path parameter cannot be empty.";
            parsedId = Guid.Empty;
            return false;
        }

        if (!Guid.TryParse(claimId, out parsedId))
        {
            errorMessage = "The claimId path parameter must be a valid GUID.";
            return false;
        }

        if (parsedId == Guid.Empty)
        {
            errorMessage = "The claimId path parameter cannot be an empty GUID.";
            return false;
        }

        errorMessage = null;
        return true;
    }

}
