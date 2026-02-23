using FluentAssertions;
using MinecraftSkins.Application.Dtos;

namespace MinecraftSkins.Tests.Application.DtoTests;

public class DtoTests
{
    [Fact]
    public void CreatePurchaseDto_ShouldSetProperties()
    {
        var skinId = Guid.NewGuid();
        var dto = new CreatePurchaseDto { SkinId = skinId };
        dto.SkinId.Should().Be(skinId);
    }

    [Fact]
    public void CreateSkinDto_ShouldSetProperties()
    {
        var dto = new CreateSkinDto
        {
            Name = "Test Skin",
            BasePriceUsd = 19.99m,
            IsAvailable = true
        };
        dto.Name.Should().Be("Test Skin");
        dto.BasePriceUsd.Should().Be(19.99m);
        dto.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void PurchaseDto_ShouldSetProperties()
    {
        var id = Guid.NewGuid();
        var skinId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var dto = new PurchaseDto
        {
            Id = id,
            SkinId = skinId,
            SkinName = "Cool Skin",
            PriceUsdFinal = 100.50m,
            BtcUsdRate = 50000m,
            RateSource = "External",
            PurchasedAtUtc = now,
            BuyerId = "user123"
        };
        dto.Id.Should().Be(id);
        dto.SkinId.Should().Be(skinId);
        dto.SkinName.Should().Be("Cool Skin");
        dto.PriceUsdFinal.Should().Be(100.50m);
        dto.BtcUsdRate.Should().Be(50000m);
        dto.RateSource.Should().Be("External");
        dto.PurchasedAtUtc.Should().Be(now);
        dto.BuyerId.Should().Be("user123");
    }

    [Fact]
    public void PurchaseResponseDto_ShouldSetProperties()
    {
        var id = Guid.NewGuid();
        var skinId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var dto = new PurchaseResponseDto
        {
            Id = id,
            SkinId = skinId,
            FinalPrice = 99.99m,
            BtcRate = 48000m,
            RateSource = "Cache",
            PurchasedAt = now
        };
        dto.Id.Should().Be(id);
        dto.SkinId.Should().Be(skinId);
        dto.FinalPrice.Should().Be(99.99m);
        dto.BtcRate.Should().Be(48000m);
        dto.RateSource.Should().Be("Cache");
        dto.PurchasedAt.Should().Be(now);
    }

    [Fact]
    public void RateDto_ShouldSetProperties()
    {
        var now = DateTime.UtcNow;
        var dto = new RateDto
        {
            Rate = 50000m,
            AsOfUtc = now,
            Source = "Test",
            AgeSeconds = 5
        };
        dto.Rate.Should().Be(50000m);
        dto.AsOfUtc.Should().Be(now);
        dto.Source.Should().Be("Test");
        dto.AgeSeconds.Should().Be(5);
    }

    [Fact]
    public void SkinDto_ShouldSetProperties()
    {
        var id = Guid.NewGuid();
        var dto = new SkinDto
        {
            Id = id,
            Name = "Dragon",
            BasePriceUsd = 29.99m,
            FinalPriceUsd = 45.50m,
            IsAvailable = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        dto.Id.Should().Be(id);
        dto.Name.Should().Be("Dragon");
        dto.BasePriceUsd.Should().Be(29.99m);
        dto.FinalPriceUsd.Should().Be(45.50m);
        dto.IsAvailable.Should().BeTrue();
        dto.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        dto.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateSkinDto_ShouldSetProperties()
    {
        var dto = new UpdateSkinDto
        {
            Name = "Updated",
            BasePriceUsd = 15.75m,
            IsAvailable = false
        };
        dto.Name.Should().Be("Updated");
        dto.BasePriceUsd.Should().Be(15.75m);
        dto.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void UpdateSkinDto_DefaultValues_ShouldBeNull()
    {
        var dto = new UpdateSkinDto();
        dto.Name.Should().BeNull();
        dto.BasePriceUsd.Should().BeNull();
        dto.IsAvailable.Should().BeNull();
    }
}