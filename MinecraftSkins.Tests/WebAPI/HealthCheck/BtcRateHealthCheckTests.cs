using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.WebAPI.HealthChecks;

namespace MinecraftSkins.Tests.WebAPI.HealthChecks;

public class BtcRateHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_ReturnsHealthy_WhenRateServiceWorks()
    {
        var mockRateService = new Mock<IBtcRateService>();
        var rateResult = new BtcRateResult { Rate = 50000, Source = "Test", AsOfUtc = DateTime.UtcNow, AgeSeconds = 5 };
        mockRateService.Setup(r => r.GetCurrentRateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rateResult);

        var logger = Mock.Of<ILogger<BtcRateHealthCheck>>();
        var check = new BtcRateHealthCheck(mockRateService.Object, logger);
        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("rate");
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsDegraded_WhenServiceThrows()
    {
        var mockRateService = new Mock<IBtcRateService>();
        mockRateService.Setup(r => r.GetCurrentRateAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service unavailable"));

        var logger = Mock.Of<ILogger<BtcRateHealthCheck>>();
        var check = new BtcRateHealthCheck(mockRateService.Object, logger);
        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Degraded);
    }
}