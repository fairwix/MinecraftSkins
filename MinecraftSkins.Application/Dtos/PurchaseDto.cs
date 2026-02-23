using FluentValidation;

namespace MinecraftSkins.Application.Dtos;

public class PurchaseDto
{
    public Guid Id { get; set; }
    public Guid SkinId { get; set; }
    public string SkinName { get; set; } = string.Empty;
    public decimal PriceUsdFinal { get; set; }
    public decimal BtcUsdRate { get; set; }
    public string RateSource { get; set; } = string.Empty; // ← новое поле
    public DateTime PurchasedAtUtc { get; set; }
    public string BuyerId { get; set; } = string.Empty;
}

public class CreatePurchaseDto
{
    public Guid SkinId { get; set; }
}

public class PurchaseResponseDto
{
    public Guid Id { get; set; }
    public Guid SkinId { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal BtcRate { get; set; }
    public string RateSource { get; set; } = string.Empty; // ← новое поле
    public DateTime PurchasedAt { get; set; }
}
