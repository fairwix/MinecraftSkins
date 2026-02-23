using Microsoft.AspNetCore.Mvc;
using MinecraftSkins.Application.Dtos;
using MinecraftSkins.Application.Interfaces;

namespace MinecraftSkins.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatesController : ControllerBase
{
    private readonly IBtcRateService _btcRateService;

    public RatesController(IBtcRateService btcRateService)
    {
        _btcRateService = btcRateService;
    }

    [HttpGet("btc-usd")]
    public async Task<ActionResult<RateDto>> GetBtcUsdRate(CancellationToken cancellationToken)
    {
        var rateResult = await _btcRateService.GetCurrentRateAsync(cancellationToken);
        
        return Ok(new RateDto
        {
            Rate = rateResult.Rate,
            AsOfUtc = rateResult.AsOfUtc,
            Source = rateResult.Source,
            AgeSeconds = rateResult.AgeSeconds
        });
    }
}