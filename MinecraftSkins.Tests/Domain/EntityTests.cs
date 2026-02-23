using FluentAssertions;
using MinecraftSkins.Domain.Entities;

namespace MinecraftSkins.Tests.Domain;

public class EntityTests
{
    [Fact]
    public void Skin_ShouldInitializeCorrectly()
    {
        var id = Guid.NewGuid();
        var name = "Test Skin";
        var price = 19.99m;
        var now = DateTime.UtcNow;

        var skin = new Skin
        {
            Id = id,
            Name = name,
            BasePriceUsd = price,
            IsAvailable = true,
            CreatedAtUtc = now
        };

        skin.Id.Should().Be(id);
        skin.Name.Should().Be(name);
        skin.BasePriceUsd.Should().Be(price);
        skin.IsAvailable.Should().BeTrue();
        skin.CreatedAtUtc.Should().Be(now);
        skin.UpdatedAtUtc.Should().BeNull();
        skin.IsDeleted.Should().BeFalse();
        skin.DeletedAtUtc.Should().BeNull();
        skin.RowVersion.Should().BeEmpty();
    }

    [Fact]
    public void Purchase_ShouldInitializeCorrectly()
    {
        var id = Guid.NewGuid();
        var skinId = Guid.NewGuid();
        var price = 25.50m;
        var rate = 45000m;
        var source = "External";
        var buyerId = "user123";
        var purchasedAt = DateTime.UtcNow;

        var purchase = new Purchase
        {
            Id = id,
            SkinId = skinId,
            PriceUsdFinal = price,
            BtcUsdRate = rate,
            RateSource = source,
            PurchasedAtUtc = purchasedAt,
            BuyerId = buyerId
        };

        purchase.Id.Should().Be(id);
        purchase.SkinId.Should().Be(skinId);
        purchase.PriceUsdFinal.Should().Be(price);
        purchase.BtcUsdRate.Should().Be(rate);
        purchase.RateSource.Should().Be(source);
        purchase.PurchasedAtUtc.Should().Be(purchasedAt);
        purchase.BuyerId.Should().Be(buyerId);
        purchase.IdempotencyKey.Should().BeNull();
        purchase.RowVersion.Should().BeEmpty();
    }
}