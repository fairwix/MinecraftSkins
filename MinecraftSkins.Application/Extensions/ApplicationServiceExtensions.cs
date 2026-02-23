using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Application.Mappings;
using MinecraftSkins.Application.Services;
using MinecraftSkins.Application.Validators;

namespace MinecraftSkins.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddValidatorsFromAssemblyContaining<CreateSkinValidator>();
        services.AddScoped<ISkinService, SkinService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IPriceCalculator, StandardPriceCalculator>();
        services.AddValidatorsFromAssemblyContaining<CreateSkinValidator>();
        return services;
    }
}