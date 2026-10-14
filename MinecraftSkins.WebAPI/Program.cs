using Microsoft.EntityFrameworkCore;
using MinecraftSkins.WebAPI.Extensions;
using MinecraftSkins.WebAPI.Middleware;
using MinecraftSkins.Application.Extensions;
using MinecraftSkins.Infrastructure.Extensions;
using MinecraftSkins.WebAPI.HealthChecks;
using MinecraftSkins.Infrastructure.Data;
using MinecraftSkins.WebAPI.Authentication;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var configuredApiKeys = builder.Configuration.GetSection("Authentication:ApiKeys").GetChildren().ToArray();
if (configuredApiKeys.Length == 0 || !configuredApiKeys.Any(key =>
        string.Equals(key["Role"], "Admin", StringComparison.OrdinalIgnoreCase)))
{
    throw new InvalidOperationException(
        "Configure at least one admin API key under Authentication:ApiKeys before starting the API.");
}

var seenApiKeys = new HashSet<string>(StringComparer.Ordinal);
foreach (var key in configuredApiKeys)
{
    if (string.IsNullOrWhiteSpace(key["Key"]) ||
        System.Text.Encoding.UTF8.GetByteCount(key["Key"]!) < 32 ||
        key["Key"] != key["Key"]!.Trim() ||
        !seenApiKeys.Add(key["Key"]!) ||
        string.IsNullOrWhiteSpace(key["UserId"]) ||
        !(string.Equals(key["Role"], "Admin", StringComparison.OrdinalIgnoreCase) ||
          string.Equals(key["Role"], "User", StringComparison.OrdinalIgnoreCase)))
    {
        throw new InvalidOperationException(
            $"Authentication:ApiKeys:{key.Key} must define a 32-byte Key, UserId, and Role (Admin or User).");
    }
}

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebAPIServices();

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddAuthentication("ApiKey")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        "ApiKey", _ => { });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<BtcRateHealthCheck>("btc_rate");
builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(60);
    options.MaximumHistoryEntriesPerEndpoint(10);
}).AddInMemoryStorage();

var app = builder.Build();
var migrateOnly = args.Contains("--migrate-only", StringComparer.Ordinal);

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Environment.IsDevelopment() || migrateOnly)
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Есть неприменённые миграции: {Migrations}", pendingMigrations);
        }
    }
}

if (migrateOnly)
    return;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.Use(async (context, next) =>
{
    var instanceId = Environment.GetEnvironmentVariable("INSTANCE_ID") ?? "unknown";
    context.Response.Headers.Append("X-Instance-Id", instanceId);
    await next();
});
app.UseHttpMetrics();
app.MapMetrics();     
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health-ui");
app.MapHealthChecksUI();

app.Run();
