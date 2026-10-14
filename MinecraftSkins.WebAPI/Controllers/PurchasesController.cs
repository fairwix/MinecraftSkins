using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MinecraftSkins.Application.Dtos;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Application.Mappings;
using MinecraftSkins.Application.Services;

namespace MinecraftSkins.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly ISkinService _skinService;
    private readonly IBtcRateService _btcRateService;
    private readonly ILogger<PurchasesController> _logger;
    private readonly IValidator<CreatePurchaseDto> _validator;
    

    public PurchasesController(
        IPurchaseService purchaseService,
        ISkinService skinService,
        IBtcRateService btcRateService,
        ILogger<PurchasesController> logger, IValidator<CreatePurchaseDto> validator)
    
    {
        _purchaseService = purchaseService;
        _skinService = skinService;
        _btcRateService = btcRateService;
        _logger = logger;
        _validator = validator;
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseResponseDto>> CreatePurchase(
        CreatePurchaseDto purchaseDto, 
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(purchaseDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();
        
        var rateResult = await _btcRateService.GetCurrentRateAsync(cancellationToken);
        
        var purchase = await _purchaseService.CreatePurchaseAsync(
            purchaseDto.SkinId, 
            userId, 
            rateResult.Rate, 
            rateResult.Source,
            idempotencyKey,
            cancellationToken);
        
        if (purchase == null)
            return StatusCode(500, "Failed to create purchase");
        
        var response = purchase.ToResponseDto();
    
        return CreatedAtAction(nameof(GetPurchase), new { id = purchase.Id }, response);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseDto>> GetPurchase(Guid id, CancellationToken cancellationToken)
    {
        var purchase = await _purchaseService.GetByIdAsync(id, cancellationToken);
        if (purchase == null)
            return NotFound();

        if (!User.IsInRole("Admin") && purchase.BuyerId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
            return NotFound();

        var purchaseDto = purchase.ToDto();
        return Ok(purchaseDto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseDto>>> GetPurchases(
        [FromQuery] bool mineOnly = false,
        [FromQuery] Guid? skinId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var callerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(callerId))
            return Unauthorized();

        // A regular user can only read their own purchases, regardless of query parameters.
        // Admin keys may omit mineOnly to inspect all purchases.
        string? buyerId = User.IsInRole("Admin") && !mineOnly ? null : callerId;

        var purchases = await _purchaseService.GetPurchasesAsync(
            buyerId, skinId, from, to, skip, take, cancellationToken);

        var purchaseDtos = purchases.Select(purchase => purchase.ToDto()).ToList();
        return Ok(purchaseDtos);
    }
}
