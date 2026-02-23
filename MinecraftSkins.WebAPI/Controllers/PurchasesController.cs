using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MinecraftSkins.Application.Dtos;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Application.Services;

namespace MinecraftSkins.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly ISkinService _skinService;
    private readonly IBtcRateService _btcRateService;
    private readonly IMapper _mapper;
    private readonly ILogger<PurchasesController> _logger;
    private readonly IValidator<CreatePurchaseDto> _validator;
    

    public PurchasesController(
        IPurchaseService purchaseService,
        ISkinService skinService,
        IBtcRateService btcRateService,
        IMapper mapper, ILogger<PurchasesController> logger, IValidator<CreatePurchaseDto> validator)
    
    {
        _purchaseService = purchaseService;
        _skinService = skinService;
        _btcRateService = btcRateService;
        _mapper = mapper;
        _logger = logger;
        _validator = validator;
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseResponseDto>> CreatePurchase(
        CreatePurchaseDto purchaseDto, 
        [FromHeader(Name = "X-User-Id")] string userId,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(purchaseDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("X-User-Id header is required");
        
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
        
        var response = _mapper.Map<PurchaseResponseDto>(purchase);
    
        return CreatedAtAction(nameof(GetPurchase), new { id = purchase.Id }, response);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseDto>> GetPurchase(Guid id, CancellationToken cancellationToken)
    {
        var purchase = await _purchaseService.GetByIdAsync(id, cancellationToken);
        if (purchase == null)
            return NotFound();

        var purchaseDto = _mapper.Map<PurchaseDto>(purchase);
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
        string? buyerId = null;
        if (mineOnly)
        {
            if (!Request.Headers.TryGetValue("X-User-Id", out var userIdValues) || string.IsNullOrWhiteSpace(userIdValues))
                return BadRequest("X-User-Id header is required for mineOnly filter");
            
            buyerId = userIdValues.ToString();
        }

        var purchases = await _purchaseService.GetPurchasesAsync(
            buyerId, skinId, from, to, skip, take, cancellationToken);

        var purchaseDtos = _mapper.Map<List<PurchaseDto>>(purchases);
        return Ok(purchaseDtos);
    }
}