using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MinecraftSkins.Application.Exceptions;
using MinecraftSkins.Application.Interfaces;

namespace MinecraftSkins.Infrastructure.Services;

public class BtcRateService : IBtcRateService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BtcRateService> _logger;
    private readonly string _cacheKey = "btc_usd_rate";
    private readonly string _fallbackCacheKey = "btc_usd_rate_fallback";
    private readonly TimeSpan _cacheTtl = TimeSpan.FromSeconds(60);
    private readonly TimeSpan _fallbackMaxAge = TimeSpan.FromMinutes(10);

    public BtcRateService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<BtcRateService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BtcRateResult> GetCurrentRateAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(_cacheKey, out BtcRateResult? cachedRate) && cachedRate != null)
        {
            _logger.LogDebug("Returning cached BTC rate: {Rate}", cachedRate.Rate);
            return new BtcRateResult
            {
                Rate = cachedRate.Rate,
                AsOfUtc = cachedRate.AsOfUtc,
                Source = "Cache",
                AgeSeconds = (int)(DateTime.UtcNow - cachedRate.AsOfUtc).TotalSeconds
            };
        }
        
        try
        {
            var rate = await FetchRateFromApiAsync(cancellationToken);
            var now = DateTime.UtcNow;
            
            var result = new BtcRateResult
            {
                Rate = rate,
                AsOfUtc = now,
                Source = "External"
            };
            _cache.Set(_cacheKey, result, _cacheTtl);
            
            _cache.Set(_fallbackCacheKey, result, _fallbackMaxAge);

            _logger.LogInformation("Fetched fresh BTC rate from API: {Rate}", rate);
            return result;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch BTC rate from API");
            
            if (_cache.TryGetValue(_fallbackCacheKey, out BtcRateResult? fallback) && fallback != null)
            {
                var age = DateTime.UtcNow - fallback.AsOfUtc;
                if (age <= _fallbackMaxAge)
                {
                    _logger.LogWarning("Using fallback BTC rate from {Time}, age: {Age} seconds", 
                        fallback.AsOfUtc, age.TotalSeconds);
                    
                    return new BtcRateResult
                    {
                        Rate = fallback.Rate,
                        AsOfUtc = fallback.AsOfUtc,
                        Source = "Fallback",
                        AgeSeconds = (int)age.TotalSeconds
                    };
                }
            }
            
            throw new ExternalServiceUnavailableException(
                "CoinGecko", 
                "BTC rate service unavailable and no fresh fallback available", 
                ex);
        }
    }

    private async Task<decimal> FetchRateFromApiAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("simple/price?ids=bitcoin&vs_currencies=usd", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(json);
        var rate = document.RootElement.GetProperty("bitcoin").GetProperty("usd").GetDecimal();
        
        return rate;
    }
}