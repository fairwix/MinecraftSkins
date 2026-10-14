using FluentAssertions;
using MinecraftSkins.Application.Dtos;
using MinecraftSkins.Application.Mappings;
using MinecraftSkins.Domain.Entities;

namespace MinecraftSkins.Tests.Application.Mappings;

public class DtoMappingsTests
{
    [Fact]
    public void ToDto_ShouldMapSkinWithoutInternalFields()
    {
        var skin = new Skin
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            BasePriceUsd = 10,
            IsAvailable = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = true,
            DeletedAtUtc = DateTime.UtcNow,
            RowVersion = [1, 2, 3]
        };

        var dto = skin.ToDto();

        dto.Id.Should().Be(skin.Id);
        dto.Name.Should().Be(skin.Name);
        dto.BasePriceUsd.Should().Be(skin.BasePriceUsd);
        dto.IsAvailable.Should().Be(skin.IsAvailable);
        dto.CreatedAtUtc.Should().Be(skin.CreatedAtUtc);
        dto.UpdatedAtUtc.Should().Be(skin.UpdatedAtUtc);
    }

    [Fact]
    public void ToEntity_ShouldMapOnlyCreateSkinFields()
    {
        var dto = new CreateSkinDto { Name = "New", BasePriceUsd = 15, IsAvailable = true };

        var skin = dto.ToEntity();

        skin.Name.Should().Be(dto.Name);
        skin.BasePriceUsd.Should().Be(dto.BasePriceUsd);
        skin.IsAvailable.Should().Be(dto.IsAvailable);
        skin.Id.Should().BeEmpty();
        skin.CreatedAtUtc.Should().Be(default);
        skin.RowVersion.Should().BeEmpty();
    }

    [Fact]
    public void ToDto_ShouldMapPurchaseAndUseEmptySkinNameWhenNavigationIsMissing()
    {
        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            SkinId = Guid.NewGuid(),
            PriceUsdFinal = 100,
            BtcUsdRate = 50000,
            RateSource = "External",
            PurchasedAtUtc = DateTime.UtcNow,
            BuyerId = "user"
        };

        var dto = purchase.ToDto();

        dto.Id.Should().Be(purchase.Id);
        dto.SkinId.Should().Be(purchase.SkinId);
        dto.SkinName.Should().BeEmpty();
        dto.PriceUsdFinal.Should().Be(purchase.PriceUsdFinal);
        dto.BtcUsdRate.Should().Be(purchase.BtcUsdRate);
        dto.RateSource.Should().Be(purchase.RateSource);
        dto.PurchasedAtUtc.Should().Be(purchase.PurchasedAtUtc);
        dto.BuyerId.Should().Be(purchase.BuyerId);
    }

    [Fact]
    public void ToResponseDto_ShouldMapPurchaseResponseFields()
    {
        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            SkinId = Guid.NewGuid(),
            PriceUsdFinal = 123.45m,
            BtcUsdRate = 55000m,
            RateSource = "Cache",
            PurchasedAtUtc = DateTime.UtcNow
        };

        var dto = purchase.ToResponseDto();

        dto.Id.Should().Be(purchase.Id);
        dto.SkinId.Should().Be(purchase.SkinId);
        dto.FinalPrice.Should().Be(purchase.PriceUsdFinal);
        dto.BtcRate.Should().Be(purchase.BtcUsdRate);
        dto.RateSource.Should().Be(purchase.RateSource);
        dto.PurchasedAt.Should().Be(purchase.PurchasedAtUtc);
    }
}
