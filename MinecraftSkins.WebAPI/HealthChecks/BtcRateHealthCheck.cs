using Microsoft.Extensions.Diagnostics.HealthChecks;
using MinecraftSkins.Application.Interfaces;

namespace MinecraftSkins.WebAPI.HealthChecks;

public class BtcRateHealthCheck : IHealthCheck
{
    private readonly IBtcRateService _btcRateService;
    private readonly ILogger<BtcRateHealthCheck> _logger;

    public BtcRateHealthCheck(IBtcRateService btcRateService, ILogger<BtcRateHealthCheck> logger)
    {
        _btcRateService = btcRateService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            
            var rate = await _btcRateService.GetCurrentRateAsync(cts.Token);
            
            var data = new Dictionary<string, object>
            {
                { "rate", rate.Rate },
                { "source", rate.Source },
                { "age_seconds", rate.AgeSeconds ?? 0 },
                { "as_of_utc", rate.AsOfUtc }
            };
            
            return HealthCheckResult.Healthy("BTC rate service is available", data);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "BTC rate health check failed");
            return HealthCheckResult.Degraded("BTC rate service is unavailable", ex);
        }
    }
}