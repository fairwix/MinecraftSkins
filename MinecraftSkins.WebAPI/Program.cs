using Microsoft.EntityFrameworkCore;
using MinecraftSkins.WebAPI.Extensions;
using MinecraftSkins.WebAPI.Middleware;
using MinecraftSkins.Application.Extensions;
using MinecraftSkins.Infrastructure.Extensions;
using MinecraftSkins.WebAPI.HealthChecks;
using MinecraftSkins.Infrastructure.Data;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebAPIServices();

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<BtcRateHealthCheck>("btc_rate");
builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(60);
    options.MaximumHistoryEntriesPerEndpoint(10);
}).AddInMemoryStorage();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Environment.IsDevelopment())
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
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