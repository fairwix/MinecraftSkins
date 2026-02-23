using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Application.Services;
using MinecraftSkins.Infrastructure.Data;
using MinecraftSkins.Infrastructure.Services;

namespace MinecraftSkins.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddHttpClient<IBtcRateService, BtcRateService>(client =>
        {
            client.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "MinecraftSkins/1.0");
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        return services;
    }
}