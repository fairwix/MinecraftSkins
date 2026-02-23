using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Infrastructure.Data;
using MinecraftSkins.Infrastructure.Extensions;
using MinecraftSkins.Infrastructure.Services;

namespace MinecraftSkins.Tests.Infrastructure.Extensions;

public class InfrastructureServiceExtensionsTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterDbContextAndHttpClient()
    {
        var services = new ServiceCollection();
        
        services.AddMemoryCache();
        
        var inMemorySettings = new Dictionary<string, string?> {
            {"ConnectionStrings:DefaultConnection", "Host=localhost;Database=fake;"}
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();
        
        serviceProvider.GetService<AppDbContext>().Should().NotBeNull();
        serviceProvider.GetService<IAppDbContext>().Should().NotBeNull();
        
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull();
        
        var btcRateService = serviceProvider.GetService<IBtcRateService>();
        btcRateService.Should().NotBeNull();
    }
}