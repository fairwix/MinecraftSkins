using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MinecraftSkins.Domain.Entities;

namespace MinecraftSkins.Infrastructure.Data.Configurations;

public class SkinConfiguration : IEntityTypeConfiguration<Skin>
{
    public void Configure(EntityTypeBuilder<Skin> builder)
    {
        builder.ToTable("Skins");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(x => x.BasePriceUsd)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(x => x.IsAvailable)
            .IsRequired();
        
        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
        
        builder.Property(x => x.UpdatedAtUtc);
        
        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(x => x.DeletedAtUtc);
        
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.IsAvailable);
        builder.HasIndex(x => x.IsDeleted);
        
        builder.Property(s => s.RowVersion)
            .HasColumnType("bytea")
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasDefaultValueSql("''");
    }
}