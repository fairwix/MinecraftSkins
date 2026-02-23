using Microsoft.EntityFrameworkCore;
using MinecraftSkins.Domain.Entities;

namespace MinecraftSkins.Infrastructure.Data;

public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var skins = new[]
        {
            new Skin
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Creeper Classic",
                BasePriceUsd = 9.99m,
                IsAvailable = true,
                CreatedAtUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Skin
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Enderman Elite",
                BasePriceUsd = 14.99m,
                IsAvailable = true,
                CreatedAtUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Skin
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Dragon Scale",
                BasePriceUsd = 19.99m,
                IsAvailable = true,
                CreatedAtUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            },
            new Skin
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Piglin Warrior",
                BasePriceUsd = 12.99m,
                IsAvailable = false,
                CreatedAtUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        };
        modelBuilder.Entity<Skin>().HasData(skins);
    }
}