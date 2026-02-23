using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MinecraftSkins.WebAPI.Extensions;

namespace MinecraftSkins.Tests.WebAPI.Extensions;

public class ServiceExtensionsTests
{
    [Fact]
    public async Task AddWebAPIServices_ShouldRegisterCorsPolicy()
    {
        var services = new ServiceCollection();
        
        services.AddLogging();
        
        services.AddWebAPIServices();
        var serviceProvider = services.BuildServiceProvider();
        
        var corsService = serviceProvider.GetService<ICorsService>();
        corsService.Should().NotBeNull();
        
        var corsPolicyProvider = serviceProvider.GetService<ICorsPolicyProvider>();
        corsPolicyProvider.Should().NotBeNull();
        
        var mockHttpContext = new Mock<HttpContext>();
        
        var policy = await corsPolicyProvider!.GetPolicyAsync(mockHttpContext.Object, "AllowAll");
        
        policy.Should().NotBeNull();
        policy!.AllowAnyOrigin.Should().BeTrue();
        policy.AllowAnyMethod.Should().BeTrue();
        policy.AllowAnyHeader.Should().BeTrue();
    }

    [Fact]
    public void AddWebAPIServices_ShouldReturnServiceCollection_ForChaining()
    {
        var services = new ServiceCollection();
        
        var result = services.AddWebAPIServices();
        
        result.Should().BeSameAs(services);
    }
}