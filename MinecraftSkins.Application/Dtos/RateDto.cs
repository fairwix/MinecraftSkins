namespace MinecraftSkins.Application.Dtos;

public class RateDto
{
    public decimal Rate { get; set; }
    public DateTime AsOfUtc { get; set; }
    public string Source { get; set; } = string.Empty;
    public int? AgeSeconds { get; set; }
}