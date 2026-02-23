namespace MinecraftSkins.Application.Interfaces;

public interface IBtcRateService
{
    Task<BtcRateResult> GetCurrentRateAsync(CancellationToken cancellationToken);
}

public class BtcRateResult
{
    public decimal Rate { get; set; }
    public DateTime AsOfUtc { get; set; }
    public string Source { get; set; } = string.Empty;
    public int? AgeSeconds { get; set; }
}