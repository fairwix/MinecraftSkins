using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using MinecraftSkins.Application.Exceptions;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Application.Services;
using MinecraftSkins.Domain.Entities;
using MinecraftSkins.Infrastructure.Data;

namespace MinecraftSkins.Tests.Application.Services;

public class PurchaseServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PurchaseService _service;
    private readonly Mock<IPriceCalculator> _mockCalculator;
    private readonly ILogger<PurchaseService> _logger;

    public PurchaseServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => 
                warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _context = new AppDbContext(options);
        _mockCalculator = new Mock<IPriceCalculator>();
        _logger = new LoggerFactory().CreateLogger<PurchaseService>();
        
        _service = new PurchaseService(_context, _logger, _mockCalculator.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreatePurchaseAsync_WithValidData_CreatesPurchase()
    {
        var skin = new Skin
        {
            Id = Guid.NewGuid(),
            Name = "Test Skin",
            BasePriceUsd = 100,
            IsAvailable = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow
        };
        
        await _context.Skins.AddAsync(skin);
        await _context.SaveChangesAsync();

        var buyerId = "user123";
        var btcRate = 50000m;
        var rateSource = "External";
        var idempotencyKey = "idem-123";
        var expectedPrice = 150m;

        _mockCalculator.Setup(c => c.CalculateFinalPrice(skin.BasePriceUsd, btcRate))
            .Returns(expectedPrice);
        
        var purchase = await _service.CreatePurchaseAsync(
            skin.Id, buyerId, btcRate, rateSource, idempotencyKey, CancellationToken.None);
        
        purchase.Should().NotBeNull();
        purchase!.SkinId.Should().Be(skin.Id);
        purchase.BuyerId.Should().Be(buyerId);
        purchase.BtcUsdRate.Should().Be(btcRate);
        purchase.RateSource.Should().Be(rateSource);
        purchase.IdempotencyKey.Should().Be(idempotencyKey);
        purchase.PriceUsdFinal.Should().Be(expectedPrice);
        purchase.PurchasedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var savedPurchase = await _context.Purchases
            .Include(p => p.Skin)
            .FirstOrDefaultAsync(p => p.Id == purchase.Id);
        
        savedPurchase.Should().NotBeNull();
        savedPurchase!.Skin!.Name.Should().Be(skin.Name);
    }

    [Fact]
    public async Task CreatePurchaseAsync_WithExistingIdempotencyKey_ReturnsExisting()
    {
        var skin = new Skin
        {
            Id = Guid.NewGuid(),
            Name = "Test Skin",
            BasePriceUsd = 100,
            IsAvailable = true,
            IsDeleted = false
        };
        
        var idempotencyKey = "existing-key";
        var existingPurchase = new Purchase
        {
            Id = Guid.NewGuid(),
            SkinId = skin.Id,
            BuyerId = "user123",
            BtcUsdRate = 50000,
            RateSource = "External",
            IdempotencyKey = idempotencyKey,
            PriceUsdFinal = 150,
            PurchasedAtUtc = DateTime.UtcNow.AddMinutes(-5),
            Skin = skin
        };

        await _context.Skins.AddAsync(skin);
        await _context.Purchases.AddAsync(existingPurchase);
        await _context.SaveChangesAsync();
        
        var result = await _service.CreatePurchaseAsync(
            skin.Id, "different-user", 60000, "Cache", idempotencyKey, CancellationToken.None);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingPurchase.Id);
        result.BuyerId.Should().Be(existingPurchase.BuyerId);
        result.BtcUsdRate.Should().Be(existingPurchase.BtcUsdRate);

        var purchasesCount = await _context.Purchases.CountAsync();
        purchasesCount.Should().Be(1);
    }

    [Fact]
    public async Task CreatePurchaseAsync_SkinNotFound_ThrowsSkinUnavailableException()
    {
        var nonExistentSkinId = Guid.NewGuid();
        
        Func<Task> act = async () => await _service.CreatePurchaseAsync(
            nonExistentSkinId, "user", 50000, "External", null, CancellationToken.None);
        
        await act.Should().ThrowAsync<SkinUnavailableException>()
            .WithMessage($"*{nonExistentSkinId}*not found*");
    }

    [Fact]
    public async Task CreatePurchaseAsync_SkinNotAvailable_ThrowsSkinUnavailableException()
    {
        var skin = new Skin
        {
            Id = Guid.NewGuid(),
            Name = "Unavailable Skin",
            BasePriceUsd = 100,
            IsAvailable = false,
            IsDeleted = false
        };

        await _context.Skins.AddAsync(skin);
        await _context.SaveChangesAsync();
        
        Func<Task> act = async () => await _service.CreatePurchaseAsync(
            skin.Id, "user", 50000, "External", null, CancellationToken.None);
        
        await act.Should().ThrowAsync<SkinUnavailableException>()
            .WithMessage($"*{skin.Id}*not available*");
    }

    [Fact]
    public async Task CreatePurchaseAsync_SkinDeleted_ThrowsSkinUnavailableException()
    {
        var skin = new Skin
        {
            Id = Guid.NewGuid(),
            Name = "Deleted Skin",
            BasePriceUsd = 100,
            IsAvailable = true,
            IsDeleted = true,
            DeletedAtUtc = DateTime.UtcNow
        };

        await _context.Skins.AddAsync(skin);
        await _context.SaveChangesAsync();
        
        Func<Task> act = async () => await _service.CreatePurchaseAsync(
            skin.Id, "user", 50000, "External", null, CancellationToken.None);
        
        await act.Should().ThrowAsync<SkinUnavailableException>()
            .WithMessage($"*{skin.Id}*not found*");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPurchase_WhenExists()
    {
        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            Skin = new Skin { Name = "Test Skin" },
            BuyerId = "user123",
            PriceUsdFinal = 150,
            BtcUsdRate = 50000,
            PurchasedAtUtc = DateTime.UtcNow
        };

        await _context.Purchases.AddAsync(purchase);
        await _context.SaveChangesAsync();
        
        var result = await _service.GetByIdAsync(purchase.Id, CancellationToken.None);
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(purchase.Id);
        result.Skin.Should().NotBeNull();
        result.Skin!.Name.Should().Be("Test Skin");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdempotencyKeyAsync_ShouldReturnPurchase_WhenExists()
    {
        var idempotencyKey = "test-key-123";
        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            Skin = new Skin(),
            BuyerId = "user123"
        };

        await _context.Purchases.AddAsync(purchase);
        await _context.SaveChangesAsync();
        
        var result = await _service.GetByIdempotencyKeyAsync(idempotencyKey, CancellationToken.None);
        
        result.Should().NotBeNull();
        result!.IdempotencyKey.Should().Be(idempotencyKey);
    }

    [Fact]
    public async Task GetByIdempotencyKeyAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _service.GetByIdempotencyKeyAsync("non-existent", CancellationToken.None);
        
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPurchasesAsync_ShouldApplyFilters()
    {
        var buyerId = "buyer1";
        var skinId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var skin = new Skin { Id = skinId, Name = "Test Skin" };
        await _context.Skins.AddAsync(skin);

        var purchases = new[]
        {
            new Purchase 
            { 
                BuyerId = buyerId, 
                SkinId = skinId, 
                PurchasedAtUtc = now.AddHours(-1),
                Skin = skin,
                PriceUsdFinal = 100
            },
            new Purchase 
            { 
                BuyerId = buyerId, 
                SkinId = Guid.NewGuid(), 
                PurchasedAtUtc = now,
                Skin = new Skin(),
                PriceUsdFinal = 200
            },
            new Purchase 
            { 
                BuyerId = "other", 
                SkinId = skinId, 
                PurchasedAtUtc = now.AddHours(-2),
                Skin = skin,
                PriceUsdFinal = 300
            }
        };

        await _context.Purchases.AddRangeAsync(purchases);
        await _context.SaveChangesAsync();
        
        var result = await _service.GetPurchasesAsync(
            buyerId, skinId, now.AddHours(-3), now.AddHours(1), 0, 10, CancellationToken.None);
        
        result.Should().HaveCount(1);
        result.First().BuyerId.Should().Be(buyerId);
        result.First().SkinId.Should().Be(skinId);
        result.First().PriceUsdFinal.Should().Be(100);
    }

    [Fact]
    public async Task GetPurchasesAsync_ShouldRespectPagination()
    {
        var buyerId = "buyer1";
        
        for (int i = 0; i < 15; i++)
        {
            await _context.Purchases.AddAsync(new Purchase
            {
                Id = Guid.NewGuid(),
                BuyerId = buyerId,
                Skin = new Skin(),
                PurchasedAtUtc = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();
        
        var page1 = await _service.GetPurchasesAsync(buyerId, null, null, null, 0, 5, CancellationToken.None);
        var page2 = await _service.GetPurchasesAsync(buyerId, null, null, null, 5, 5, CancellationToken.None);
        
        page1.Should().HaveCount(5);
        page2.Should().HaveCount(5);
        page1.Should().NotBeEquivalentTo(page2);
    }

    [Fact]
    public async Task GetPurchasesAsync_ShouldReturnEmpty_WhenNoMatches()
    {
        var result = await _service.GetPurchasesAsync(
            "non-existent", null, null, null, 0, 10, CancellationToken.None);
        
        result.Should().BeEmpty();
    }
}