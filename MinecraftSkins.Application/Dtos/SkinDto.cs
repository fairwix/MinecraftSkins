namespace MinecraftSkins.Application.Dtos;

public class SkinDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePriceUsd { get; set; }
    public decimal FinalPriceUsd { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class CreateSkinDto
{
    public string Name { get; set; } = string.Empty;
    public decimal BasePriceUsd { get; set; }
    public bool IsAvailable { get; set; }
}

public class UpdateSkinDto
{
    public string? Name { get; set; }
    public decimal? BasePriceUsd { get; set; }
    public bool? IsAvailable { get; set; }
}