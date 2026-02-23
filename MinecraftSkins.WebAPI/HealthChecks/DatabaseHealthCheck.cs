using Microsoft.Extensions.Diagnostics.HealthChecks;
using MinecraftSkins.Application.Interfaces;

namespace MinecraftSkins.WebAPI.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IAppDbContext _context;

    public DatabaseHealthCheck(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            return canConnect 
                ? HealthCheckResult.Healthy("Database is available")
                : HealthCheckResult.Unhealthy("Database is unavailable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database check failed", ex);
        }
    }
}