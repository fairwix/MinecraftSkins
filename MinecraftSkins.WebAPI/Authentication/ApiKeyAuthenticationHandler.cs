using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MinecraftSkins.WebAPI.Authentication;

/// <summary>
/// Machine-to-machine authentication for the demo API. Keys must be supplied through
/// configuration (normally environment variables) and must never be committed.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationValues))
            return Task.FromResult(AuthenticateResult.NoResult());

        var authorization = authorizationValues.ToString();
        const string bearerPrefix = "Bearer ";
        if (!authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.NoResult());

        var suppliedKey = authorization[bearerPrefix.Length..].Trim();
        if (suppliedKey.Length == 0)
            return Task.FromResult(AuthenticateResult.Fail("A bearer API key is required."));

        foreach (var entry in _configuration.GetSection("Authentication:ApiKeys").GetChildren())
        {
            var configuredKey = entry["Key"];
            var userId = entry["UserId"];
            if (string.IsNullOrWhiteSpace(configuredKey) || string.IsNullOrWhiteSpace(userId))
                continue;

            var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);
            var configuredBytes = Encoding.UTF8.GetBytes(configuredKey);
            if (suppliedBytes.Length != configuredBytes.Length ||
                !CryptographicOperations.FixedTimeEquals(suppliedBytes, configuredBytes))
                continue;

            var role = string.Equals(entry["Role"], "Admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "User";
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("The bearer API key is invalid."));
    }
}
