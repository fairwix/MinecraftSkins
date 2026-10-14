using MinecraftSkins.Application.Dtos;
using MinecraftSkins.Domain.Entities;

namespace MinecraftSkins.Application.Mappings;

public static class DtoMappings
{
    public static SkinDto ToDto(this Skin skin) => new()
    {
        Id = skin.Id,
        Name = skin.Name,
        BasePriceUsd = skin.BasePriceUsd,
        IsAvailable = skin.IsAvailable,
        CreatedAtUtc = skin.CreatedAtUtc,
        UpdatedAtUtc = skin.UpdatedAtUtc
    };

    public static Skin ToEntity(this CreateSkinDto dto) => new()
    {
        Name = dto.Name,
        BasePriceUsd = dto.BasePriceUsd,
        IsAvailable = dto.IsAvailable
    };

    public static PurchaseDto ToDto(this Purchase purchase) => new()
    {
        Id = purchase.Id,
        SkinId = purchase.SkinId,
        SkinName = purchase.Skin?.Name ?? string.Empty,
        PriceUsdFinal = purchase.PriceUsdFinal,
        BtcUsdRate = purchase.BtcUsdRate,
        RateSource = purchase.RateSource,
        PurchasedAtUtc = purchase.PurchasedAtUtc,
        BuyerId = purchase.BuyerId
    };

    public static PurchaseResponseDto ToResponseDto(this Purchase purchase) => new()
    {
        Id = purchase.Id,
        SkinId = purchase.SkinId,
        FinalPrice = purchase.PriceUsdFinal,
        BtcRate = purchase.BtcUsdRate,
        RateSource = purchase.RateSource,
        PurchasedAt = purchase.PurchasedAtUtc
    };
}
