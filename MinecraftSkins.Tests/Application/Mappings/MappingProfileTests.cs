using AutoMapper;
using FluentAssertions;
using MinecraftSkins.Application.Dtos;
using MinecraftSkins.Application.Mappings;
using MinecraftSkins.Domain.Entities;

namespace MinecraftSkins.Tests.Application.Mappings;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        config.AssertConfigurationIsValid(); 
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Should_Map_Skin_To_SkinDto()
    {
        var skin = new Skin
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            BasePriceUsd = 10,
            IsAvailable = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var dto = _mapper.Map<SkinDto>(skin);

        dto.Id.Should().Be(skin.Id);
        dto.Name.Should().Be(skin.Name);
        dto.BasePriceUsd.Should().Be(skin.BasePriceUsd);
        dto.IsAvailable.Should().Be(skin.IsAvailable);
        dto.CreatedAtUtc.Should().Be(skin.CreatedAtUtc);
        dto.UpdatedAtUtc.Should().Be(skin.UpdatedAtUtc);
        dto.FinalPriceUsd.Should().Be(0);
    }

    [Fact]
    public void Should_Map_CreateSkinDto_To_Skin()
    {
        var dto = new CreateSkinDto
        {
            Name = "New",
            BasePriceUsd = 15,
            IsAvailable = true
        };

        var skin = _mapper.Map<Skin>(dto);

        skin.Name.Should().Be(dto.Name);
        skin.BasePriceUsd.Should().Be(dto.BasePriceUsd);
        skin.IsAvailable.Should().Be(dto.IsAvailable);
        skin.Id.Should().BeEmpty(); 
        skin.CreatedAtUtc.Should().Be(default);
        skin.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Should_Map_UpdateSkinDto_To_Skin()
    {
        var dto = new UpdateSkinDto
        {
            Name = "Updated",
            BasePriceUsd = 20,
            IsAvailable = false
        };
        var skin = new Skin { Name = "Old", BasePriceUsd = 10, IsAvailable = true };

        _mapper.Map(dto, skin);

        skin.Name.Should().Be("Updated");
        skin.BasePriceUsd.Should().Be(20);
        skin.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Should_Map_Purchase_To_PurchaseDto()
    {
        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            SkinId = Guid.NewGuid(),
            PriceUsdFinal = 100,
            BtcUsdRate = 50000,
            RateSource = "External",
            PurchasedAtUtc = DateTime.UtcNow,
            BuyerId = "user",
            Skin = new Skin { Name = "SkinName" }
        };

        var dto = _mapper.Map<PurchaseDto>(purchase);

        dto.Id.Should().Be(purchase.Id);
        dto.SkinId.Should().Be(purchase.SkinId);
        dto.SkinName.Should().Be("SkinName");
        dto.PriceUsdFinal.Should().Be(purchase.PriceUsdFinal);
        dto.BtcUsdRate.Should().Be(purchase.BtcUsdRate);
        dto.RateSource.Should().Be(purchase.RateSource);
        dto.PurchasedAtUtc.Should().Be(purchase.PurchasedAtUtc);
        dto.BuyerId.Should().Be(purchase.BuyerId);
    }

    [Fact]
    public void Should_Map_Purchase_To_PurchaseResponseDto()
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

        var dto = _mapper.Map<PurchaseResponseDto>(purchase);

        dto.Id.Should().Be(purchase.Id);
        dto.SkinId.Should().Be(purchase.SkinId);
        dto.FinalPrice.Should().Be(purchase.PriceUsdFinal);
        dto.BtcRate.Should().Be(purchase.BtcUsdRate);
        dto.RateSource.Should().Be(purchase.RateSource);
        dto.PurchasedAt.Should().Be(purchase.PurchasedAtUtc);
    }
}