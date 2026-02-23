using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MinecraftSkins.Domain.Entities;

namespace MinecraftSkins.Infrastructure.Data.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("Purchases");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.SkinId)
            .IsRequired();
        
        builder.Property(x => x.PriceUsdFinal)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(x => x.BtcUsdRate)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(x => x.RateSource)
            .IsRequired()
            .HasMaxLength(20) 
            .HasConversion<string>(); 
        
        builder.Property(x => x.PurchasedAtUtc)
            .IsRequired();
        
        builder.Property(x => x.BuyerId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(100);
        
        builder.Property(p => p.RowVersion)
            .HasColumnType("bytea")
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasDefaultValueSql("''");
        
        builder.HasOne(x => x.Skin)
            .WithMany()
            .HasForeignKey(x => x.SkinId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => x.SkinId);
        builder.HasIndex(x => x.BuyerId);
        builder.HasIndex(x => x.PurchasedAtUtc);
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        
        builder.HasIndex(x => x.RateSource);
    }
}