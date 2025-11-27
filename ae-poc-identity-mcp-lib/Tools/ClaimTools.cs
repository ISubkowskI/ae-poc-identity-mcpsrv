using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using Ae.Poc.Identity.Mcp.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Ae.Poc.Identity.Mcp.Tools;

/// <summary>
/// Provides a collection of MCP server tools for managing and retrieving claims.
/// This class contains static methods that expose claim-related functionality through the MCP server interface.
/// All methods return JSON-serialized responses and map entities to DTOs via AutoMapper.
/// </summary>
[McpServerToolType]
public static class ClaimTools
{
    /// <summary>
    /// Retrieves a list of claims from the claim client, maps them to DTOs, and serializes the result to JSON.
    /// </summary>
    [McpServerTool(Name = "identity-get_claims", UseStructuredContent = true)]
    [Description("Retrieve a list of claims.")]
    public static async Task<ToolResult<IEnumerable<AppClaimOutgoingDto>, ErrorOutgoingDto>> GetClaimsAsync(
        IClaimClient claimClient,
        IMapper mapper,
        //ILogger<ClaimTools>? logger = null,
        CancellationToken ct = default)
    {
        try
        {
            //logger?.LogInformation("Retrieving all claims");
            var claims = (await claimClient.LoadClaimsAsync(ct));
            if (claims == null)
            {
                //logger?.LogWarning("No claims found in the system");
                return ToolResultFactory.Warning<IEnumerable<AppClaimOutgoingDto>>("No claims found");
            }

            var res = mapper.Map<IEnumerable<AppClaimOutgoingDto>>(claims);
            if (res == null)
            {
                //logger?.LogError(ex, "Failed to map claims to outgoing DTOs");
                return ToolResultFactory.Failure<IEnumerable<AppClaimOutgoingDto>>(["Failed to map claims to outgoing DTOs"]);
            }

            //logger?.LogInformation("Successfully retrieved {ClaimCount} claims", res.Count());
            return ToolResultFactory.Success(res);
        }
        catch (Exception ex)
        {
            //logger?.LogError(ex, "Error occurred while retrieving claims");
            return ToolResultFactory.FromException<IEnumerable<AppClaimOutgoingDto>>(ex);
        }
    }

    /// <summary>
    /// Retrieves the details of a specific claim by its ID, maps it to a DTO, and serializes the result to JSON.
    /// </summary>
    [McpServerTool(Name = "identity-get_claim_details")]
    [Description("Retrieve details for a single claim by id.")]
    public static async Task<ToolResult<AppClaimOutgoingDto, ErrorOutgoingDto>> GetClaimDetailsAsync(
        [Description("The id of the claim to get details for")] string claimId,
        IClaimClient claimClient,
        IMapper mapper,
        CancellationToken ct = default)
    {
        try
        {
            if (!TryParseClaimId(claimId, out _, out var errorMessage))
            {
                return ToolResultFactory.ValidationFailed<AppClaimOutgoingDto>([errorMessage ?? "Unknown validation error"]);
            }

            var claim = await claimClient.LoadClaimDetailsAsync(claimId, ct);
            if (claim is null)
            {
                return ToolResultFactory.Warning<AppClaimOutgoingDto>("Claim not found");
            }

            var res = mapper.Map<AppClaimOutgoingDto>(claim);
            return ToolResultFactory.Success(res);
        }
        catch (Exception ex)
        {
            return ToolResultFactory.FromException<AppClaimOutgoingDto>(ex);
        }
    }

    /// <summary>
    /// Deletes a claim by its ID, maps the deleted claim to a DTO, and serializes the result to JSON.
    /// </summary>
    [McpServerTool(Name = "identity-delete_claim")]
    [Description("Delete a claim by id.")]
    public static async Task<ToolResult<AppClaimOutgoingDto, ErrorOutgoingDto>> DeleteClaimAsync(
        [Description("The id of the claim to delete")] string claimId,
        IClaimClient claimClient,
        IMapper mapper,
        CancellationToken ct = default)
    {
        try
        {
            if (!TryParseClaimId(claimId, out _, out var errorMessage))
            {
                return ToolResultFactory.ValidationFailed<AppClaimOutgoingDto>([errorMessage ?? "Unknown validation error"]);
            }

            var deletedClaim = await claimClient.DeleteClaimAsync(claimId, ct);
            var res = mapper.Map<AppClaimOutgoingDto>(deletedClaim);
            return ToolResultFactory.Success(res);
        }
        catch (Exception ex)
        {
            return ToolResultFactory.FromException<AppClaimOutgoingDto>(ex);
        }
    }

    /// <summary>
    /// Creates a new claim using the provided DTO, maps it to the entity, and serializes the created claim to JSON.
    /// </summary>
    [McpServerTool(Name = "identity-create_claim")]
    [Description("Create a new claim.")]
    public static async Task<ToolResult<AppClaimOutgoingDto, ErrorOutgoingDto>> CreateClaimAsync(
        [Description(@"The data for the new claim. The server will assign the 'Id'.
Expected JSON structure:
{
  ""Type"": ""string"",
  ""Value"": ""string"",
  ""ValueType"": ""string"",
  ""DisplayText"": ""string"",
  ""Properties"": { ""key1"": ""value1"", ... } (optional),
  ""Description"": ""string"" (optional, max 500 chars)
}")] AppClaimCreateDto claimDto,
        IClaimClient claimClient,
        IMapper mapper,
        IDtoValidator validator,
        CancellationToken ct = default)
    {
        try
        {
            if (!validator.TryValidate(claimDto, out var validationResults))
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown validation error").ToArray();
                return ToolResultFactory.ValidationFailed<AppClaimOutgoingDto>(errors);
            }

            var appClaim = mapper.Map<AppClaim>(claimDto);
            var createdClaim = await claimClient.CreateClaimAsync(appClaim, ct);
            var res = mapper.Map<AppClaimOutgoingDto>(createdClaim);
            return ToolResultFactory.Success(res);
        }
        catch (Exception ex)
        {
            return ToolResultFactory.FromException<AppClaimOutgoingDto>(ex);
        }
    }

    /// <summary>
    /// Updates an existing claim by its ID using the provided DTO, maps it to the entity, and serializes the updated claim to JSON.
    /// </summary>
    [McpServerTool(Name = "identity-update_claim")]
    [Description("Update a claim by id.")]
    public static async Task<ToolResult<AppClaimOutgoingDto, ErrorOutgoingDto>> UpdateClaimAsync(
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
}")] AppClaimUpdateDto claimDto,
        IClaimClient claimClient,
        IMapper mapper,
        IDtoValidator validator,
        CancellationToken ct = default)
    {
        try
        {
            if (!TryParseClaimId(claimId, out var parsedClaimId, out var errorMessage))
            {
                return ToolResultFactory.ValidationFailed<AppClaimOutgoingDto>([errorMessage ?? "Unknown validation error"]);
            }

            // Compare claimId with claimDto.Id
            if (parsedClaimId != claimDto.Id)
            {
                return ToolResultFactory.ValidationFailed<AppClaimOutgoingDto>(["The claimId path parameter must match the Id field in the request body."]);
            }

            if (!validator.TryValidate(claimDto, out var validationResults))
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown validation error").ToArray();
                return ToolResultFactory.ValidationFailed<AppClaimOutgoingDto>(errors);
            }

            var appClaim = mapper.Map<AppClaim>(claimDto);
            var updatedClaim = await claimClient.UpdateClaimAsync(claimId, appClaim, ct);
            var res = mapper.Map<AppClaimOutgoingDto>(updatedClaim);
            return ToolResultFactory.Success(res);
        }
        catch (Exception ex)
        {
            return ToolResultFactory.FromException<AppClaimOutgoingDto>(ex);
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
