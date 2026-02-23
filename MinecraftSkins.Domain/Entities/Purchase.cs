namespace MinecraftSkins.Domain.Entities;

public class Purchase
{
    public Guid Id { get; set; }
    public Guid SkinId { get; set; }
    public decimal PriceUsdFinal { get; set; }
    public decimal BtcUsdRate { get; set; }
    public string RateSource { get; set; } = string.Empty; 
    public DateTime PurchasedAtUtc { get; set; }
    public string BuyerId { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
    public Skin? Skin { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}