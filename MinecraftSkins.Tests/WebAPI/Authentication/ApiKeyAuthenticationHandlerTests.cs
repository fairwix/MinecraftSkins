using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MinecraftSkins.WebAPI.Authentication;
using Moq;

namespace MinecraftSkins.Tests.WebAPI.Authentication;

public class ApiKeyAuthenticationHandlerTests
{
    private const string ValidKey = "0123456789abcdef0123456789abcdef";

    [Fact]
    public async Task AuthenticateAsync_WithValidKey_ReturnsConfiguredIdentityAndRole()
    {
        var result = await AuthenticateAsync($"Bearer {ValidKey}");

        Assert.True(result.Succeeded);
        Assert.Equal("demo", result.Principal!.Identity!.Name);
        Assert.True(result.Principal.IsInRole("User"));
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidKey_FailsAuthentication()
    {
        var result = await AuthenticateAsync("Bearer invalid-key");

        Assert.False(result.Succeeded);
        Assert.Null(result.Principal);
    }

    [Fact]
    public async Task AuthenticateAsync_WithoutBearerHeader_ReturnsNoResult()
    {
        var result = await AuthenticateAsync(null);

        Assert.False(result.Succeeded);
        Assert.True(result.None);
    }

    private static async Task<AuthenticateResult> AuthenticateAsync(string? authorization)
    {
        var values = new Dictionary<string, string?>
        {
            ["Authentication:ApiKeys:0:Key"] = ValidKey,
            ["Authentication:ApiKeys:0:UserId"] = "demo",
            ["Authentication:ApiKeys:0:Role"] = "User"
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        options.Setup(monitor => monitor.Get("ApiKey")).Returns(new AuthenticationSchemeOptions());

        var handler = new ApiKeyAuthenticationHandler(
            options.Object,
            NullLoggerFactory.Instance,
            UrlEncoder.Default,
            configuration);
        var context = new DefaultHttpContext();
        if (authorization is not null)
            context.Request.Headers.Authorization = authorization;

        await handler.InitializeAsync(new AuthenticationScheme("ApiKey", "API key", typeof(ApiKeyAuthenticationHandler)), context);
        return await handler.AuthenticateAsync();
    }
}
