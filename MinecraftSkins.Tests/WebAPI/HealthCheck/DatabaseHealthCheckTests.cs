using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MinecraftSkins.Infrastructure.Data;
using MinecraftSkins.WebAPI.HealthChecks;

namespace MinecraftSkins.Tests.WebAPI.HealthChecks;

public class DatabaseHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_ReturnsHealthy_WhenDatabaseIsAvailable()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
            
        var context = new AppDbContext(options);
        var healthCheck = new DatabaseHealthCheck(context);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());
        
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Database is available");
    }
}