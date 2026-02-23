using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MinecraftSkins.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(HealthReport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthReport>> GetHealth(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);
        
        if (report.Status == HealthStatus.Healthy)
            return Ok(report);
        
        return StatusCode(503, report);
    }
    
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }
    
    [HttpGet("detailed")]
    public async Task<ActionResult<object>> GetDetailedHealth(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);
        
        var result = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration,
            timestamp = DateTime.UtcNow,
            components = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration = entry.Value.Duration,
                    data = entry.Value.Data,
                    tags = entry.Value.Tags
                }
            )
        };
        
        return Ok(result);
    }
}