using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MinecraftSkins.Application.Dtos;
using Moq;
using MinecraftSkins.Application.Extensions;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Application.Services;
using MinecraftSkins.Application.Validators;

namespace MinecraftSkins.Tests.Application.Extensions;

public class ApplicationServiceExtensionsTests
{
    [Fact]
    public void AddApplication_ShouldRegisterAllServices()
    {
        var services = new ServiceCollection();
        
        var mockDbContext = new Mock<IAppDbContext>();
        services.AddScoped(_ => mockDbContext.Object);
        
        services.AddScoped(typeof(ILogger<SkinService>), _ => Mock.Of<ILogger<SkinService>>());
        services.AddScoped(typeof(ILogger<PurchaseService>), _ => Mock.Of<ILogger<PurchaseService>>());
        
        services.AddApplication();
        var serviceProvider = services.BuildServiceProvider();
        
        serviceProvider.GetService<ISkinService>().Should().NotBeNull();
        serviceProvider.GetService<IPurchaseService>().Should().NotBeNull();
        serviceProvider.GetService<IPriceCalculator>().Should().BeOfType<StandardPriceCalculator>();
        serviceProvider.GetService<IValidator<CreateSkinDto>>().Should().BeOfType<CreateSkinValidator>();
        serviceProvider.GetService<IValidator<UpdateSkinDto>>().Should().BeOfType<UpdateSkinValidator>();
        serviceProvider.GetService<IValidator<CreatePurchaseDto>>().Should().BeOfType<CreatePurchaseValidator>();
        
        var mapper = serviceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();
    }
}