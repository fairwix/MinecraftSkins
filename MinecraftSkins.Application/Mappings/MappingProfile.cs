using AutoMapper;
using MinecraftSkins.Application.Dtos;
using MinecraftSkins.Domain.Entities;

namespace MinecraftSkins.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Skin, SkinDto>()
            .ForMember(dest => dest.FinalPriceUsd, opt => opt.Ignore());
        
        CreateMap<CreateSkinDto, Skin>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore()); // ВАЖНО!
        
        CreateMap<UpdateSkinDto, Skin>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAtUtc, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore()) // ВАЖНО!
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<Purchase, PurchaseDto>()
            .ForMember(dest => dest.SkinName,
                opt => opt.MapFrom(src => src.Skin != null ? src.Skin.Name : string.Empty));
        
        CreateMap<Purchase, PurchaseResponseDto>()
            .ForMember(dest => dest.FinalPrice, opt => opt.MapFrom(src => src.PriceUsdFinal))
            .ForMember(dest => dest.BtcRate, opt => opt.MapFrom(src => src.BtcUsdRate))
            .ForMember(dest => dest.RateSource, opt => opt.MapFrom(src => src.RateSource))
            .ForMember(dest => dest.PurchasedAt, opt => opt.MapFrom(src => src.PurchasedAtUtc));
    }
}