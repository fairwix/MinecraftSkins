namespace MinecraftSkins.Domain.Entities;

public class Skin
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePriceUsd { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}