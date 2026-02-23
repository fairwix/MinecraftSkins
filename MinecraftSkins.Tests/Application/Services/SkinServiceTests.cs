using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MinecraftSkins.Application.Services;
using MinecraftSkins.Domain.Entities;
using MinecraftSkins.Infrastructure.Data;
using MinecraftSkins.Tests.TestHelpers;

namespace MinecraftSkins.Tests.Application.Services;

public class SkinServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SkinService _service;

    public SkinServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _service = new SkinService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSkin_WhenExists()
    {
        var skin = new Skin
        {
            Id = Guid.NewGuid(),
            Name = "Test Skin",
            BasePriceUsd = 10,
            IsAvailable = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.Skins.Add(skin);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(skin.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(skin.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAndAvailableAsync_ShouldReturnTrue_WhenAvailableAndNotDeleted()
    {
        var skin = new Skin { Id = Guid.NewGuid(), Name = "Test", BasePriceUsd = 10, IsAvailable = true, IsDeleted = false };
        _context.Skins.Add(skin);
        await _context.SaveChangesAsync();

        var result = await _service.ExistsAndAvailableAsync(skin.Id, CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAndAvailableAsync_ShouldReturnFalse_WhenNotAvailable()
    {
        var skin = new Skin { Id = Guid.NewGuid(), Name = "Test", BasePriceUsd = 10, IsAvailable = false, IsDeleted = false };
        _context.Skins.Add(skin);
        await _context.SaveChangesAsync();

        var result = await _service.ExistsAndAvailableAsync(skin.Id, CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddSkin()
    {
        var skin = new Skin { Name = "New Skin", BasePriceUsd = 15, IsAvailable = true };

        var created = await _service.CreateAsync(skin, CancellationToken.None);

        created.Id.Should().NotBeEmpty();
        created.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        var dbSkin = await _context.Skins.FindAsync(created.Id);
        dbSkin.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifySkin()
    {
        var skin = new Skin { Name = "Old", BasePriceUsd = 10, IsAvailable = true };
        _context.Skins.Add(skin);
        await _context.SaveChangesAsync();

        var updated = await _service.UpdateAsync(skin.Id, "New Name", 20, false, CancellationToken.None);

        updated.Should().NotBeNull();
        updated.Name.Should().Be("New Name");
        updated.BasePriceUsd.Should().Be(20);
        updated.IsAvailable.Should().BeFalse();
        updated.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenSkinNotFound()
    {
        var result = await _service.UpdateAsync(Guid.NewGuid(), "Name", 10, true, CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesSkin()
    {
        var skin = new Skin { Name = "ToDelete", BasePriceUsd = 10 };
        _context.Skins.Add(skin);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(skin.Id, CancellationToken.None);
        result.Should().BeTrue();

        var deletedSkin = await _context.Skins.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == skin.Id);
        deletedSkin.Should().NotBeNull();
        deletedSkin!.IsDeleted.Should().BeTrue();
        deletedSkin!.DeletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSkinsAsync_ShouldApplyFilters()
    {
        _context.Skins.AddRange(
            new Skin { Name = "Alpha", IsAvailable = true, IsDeleted = false },
            new Skin { Name = "Beta", IsAvailable = false, IsDeleted = false },
            new Skin { Name = "Gamma", IsAvailable = true, IsDeleted = false },
            new Skin { Name = "Delta", IsAvailable = true, IsDeleted = true } 
        );
        await _context.SaveChangesAsync();

        var availableSkins = await _service.GetSkinsAsync(true, null, 0, 10, CancellationToken.None);
        availableSkins.Should().HaveCount(2); 

        var searched = await _service.GetSkinsAsync(false, "eta", 0, 10, CancellationToken.None);
        searched.Should().ContainSingle(s => s.Name == "Beta");
    }

    [Fact]
    public async Task GetSkinsCountAsync_ShouldReturnCorrectCount()
    {
        _context.Skins.AddRange(
            new Skin { Name = "A", IsAvailable = true },
            new Skin { Name = "B", IsAvailable = false },
            new Skin { Name = "C", IsAvailable = true }
        );
        await _context.SaveChangesAsync();

        var count = await _service.GetSkinsCountAsync(true, null, CancellationToken.None);
        count.Should().Be(2);

        var countWithSearch = await _service.GetSkinsCountAsync(false, "B", CancellationToken.None);
        countWithSearch.Should().Be(1);
    }
}