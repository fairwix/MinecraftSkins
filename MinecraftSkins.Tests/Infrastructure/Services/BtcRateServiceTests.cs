using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using MinecraftSkins.Application.Exceptions;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Infrastructure.Services;

namespace MinecraftSkins.Tests.Infrastructure.Services;

public class BtcRateServiceTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BtcRateService> _logger;
    private readonly BtcRateService _service;

    public BtcRateServiceTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("https://api.coingecko.com/") };
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = Mock.Of<ILogger<BtcRateService>>();
        _service = new BtcRateService(_httpClient, _cache, _logger);
    }

    [Fact]
    public async Task GetCurrentRateAsync_ShouldReturnCachedValue_WhenCacheExists()
    {
        var cachedRate = new BtcRateResult { Rate = 50000, AsOfUtc = DateTime.UtcNow, Source = "Cache" };
        _cache.Set("btc_usd_rate", cachedRate, TimeSpan.FromSeconds(60));

        var result = await _service.GetCurrentRateAsync(CancellationToken.None);

        result.Source.Should().Be("Cache");
        result.Rate.Should().Be(50000);
    }

    [Fact]
    public async Task GetCurrentRateAsync_ShouldFetchFromApi_WhenCacheMiss()
    {
        var responseJson = "{\"bitcoin\":{\"usd\":55000}}";
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var result = await _service.GetCurrentRateAsync(CancellationToken.None);

        result.Source.Should().Be("External");
        result.Rate.Should().Be(55000);
        result.AsOfUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _cache.TryGetValue("btc_usd_rate", out BtcRateResult cached).Should().BeTrue();
        cached.Rate.Should().Be(55000);
    }

    [Fact]
    public async Task GetCurrentRateAsync_ShouldUseFallback_WhenApiFailsAndFallbackExists()
    {
        
        var fallbackRate = new BtcRateResult { Rate = 50000, AsOfUtc = DateTime.UtcNow.AddSeconds(-30), Source = "Fallback" };
        _cache.Set("btc_usd_rate_fallback", fallbackRate, TimeSpan.FromMinutes(10));

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var result = await _service.GetCurrentRateAsync(CancellationToken.None);
        
        result.Should().NotBeNull();
        result!.Source.Should().Be("Fallback");
        result.Rate.Should().Be(50000);
        result.AgeSeconds.Should().BeLessThan(60);
    }

    [Fact]
    public async Task GetCurrentRateAsync_ShouldThrowExternalServiceUnavailable_WhenApiFailsAndNoFallback()
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        Func<Task> act = async () => await _service.GetCurrentRateAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ExternalServiceUnavailableException>()
            .WithMessage("*BTC rate service unavailable*");
    }

    [Fact]
    public async Task GetCurrentRateAsync_ShouldThrow_WhenApiReturnsNonSuccess()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        Func<Task> act = async () => await _service.GetCurrentRateAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ExternalServiceUnavailableException>();
    }

    [Fact]
    public async Task GetCurrentRateAsync_ShouldThrow_WhenJsonMalformed()
    {
        var responseJson = "invalid json";
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseJson)
        };
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        Func<Task> act = async () => await _service.GetCurrentRateAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ExternalServiceUnavailableException>();
    }
}