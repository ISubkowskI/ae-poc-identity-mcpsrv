using Ae.Poc.Identity.Mcp.Data;
using Ae.Poc.Identity.Mcp.Dtos;
using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ae.Poc.Identity.Mcp.Profiles;

/// <summary>
/// Configures AutoMapper profiles for mapping between domain entities and DTOs.
/// This profile defines how <see cref="AppClaim"/> entities are mapped to and from
/// various DTOs like <see cref="ClaimOutgoingDto"/>, <see cref="ClaimCreateDto"/>,
/// <see cref="ClaimUpdateDto"/>, and <see cref="ClaimDto"/>.
/// </summary>
public sealed class DataProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataProfile"/> class
    /// and configures the entity-DTO mappings.
    /// </summary>
    public DataProfile()
    {
        CreateMap<AppClaim, ClaimOutgoingDto>();
        CreateMap<ClaimsInfo, ClaimsInfoOutgoingDto>();
        CreateMap<ClaimCreateDto, AppClaim>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<ClaimUpdateDto, AppClaim>();

        CreateMap<ClaimDto, AppClaim>();

        CreateMap<ClaimsQueryIncomingDto, ClaimsQuery>()
            .ForMember(dest => dest.Skipped, opt => opt.MapFrom(src => (src.Skipped < 0) ? 0 : src.Skipped))
            .ForMember(dest => dest.NumberOf, opt => opt.MapFrom(src => ValidateNumberOf(src.NumberOf)));
    }

    private int ValidateNumberOf(int numberOf)
    {
        return numberOf switch
        {
            < 1 => 0,
            > 100 => 100,
            _ => numberOf
        };
    }
}
