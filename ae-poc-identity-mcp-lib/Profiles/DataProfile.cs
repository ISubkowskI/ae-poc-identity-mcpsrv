using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using AutoMapper;

namespace Ae.Poc.Identity.Mcp.Profiles;

/// <summary>
/// Configures AutoMapper profiles for mapping between domain entities and DTOs.
/// This profile defines how <see cref="AppClaim"/> entities are mapped to and from
/// various DTOs like <see cref="AppClaimOutgoingDto"/>, <see cref="AppClaimCreateDto"/>,
/// <see cref="AppClaimUpdateDto"/>, and <see cref="AppClaimDto"/>.
/// </summary>
public sealed class DataProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataProfile"/> class
    /// and configures the entity-DTO mappings.
    /// </summary>
    public DataProfile()
    {
        CreateMap<AppClaim, AppClaimOutgoingDto>();
        CreateMap<AppClaimCreateDto, AppClaim>();
        CreateMap<AppClaimUpdateDto, AppClaim>();

        CreateMap<AppClaimDto, AppClaim>();
    }
}
