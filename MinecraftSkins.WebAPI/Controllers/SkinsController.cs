using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinecraftSkins.Application.Dtos;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Application.Services;
using MinecraftSkins.Domain.Entities;

namespace MinecraftSkins.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkinsController : ControllerBase
{
    private readonly ISkinService _skinService;
    private readonly IBtcRateService _btcRateService;
    private readonly IPriceCalculator _priceCalculator;
    private readonly IMapper _mapper;

    public SkinsController(
        ISkinService skinService,
        IBtcRateService btcRateService,
        IPriceCalculator priceCalculator,
        IMapper mapper)
    {
        _skinService = skinService;
        _btcRateService = btcRateService;
        _priceCalculator = priceCalculator;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SkinDto>>> GetSkins(
        [FromQuery] bool availableOnly = false,
        [FromQuery] string? search = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var skins = await _skinService.GetSkinsAsync(
            availableOnly, 
            search, 
            skip, 
            take, 
            cancellationToken);
        
        var rateResult = await _btcRateService.GetCurrentRateAsync(cancellationToken);
        
        var skinDtos = _mapper.Map<List<SkinDto>>(skins);

        foreach (var dto in skinDtos)
        {
            dto.FinalPriceUsd = _priceCalculator.CalculateFinalPrice(dto.BasePriceUsd, rateResult.Rate);
        }

        Response.Headers.Add("X-Total-Count", 
            (await _skinService.GetSkinsCountAsync(availableOnly, search, cancellationToken)).ToString());

        return Ok(skinDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SkinDto>> GetSkin(Guid id, CancellationToken cancellationToken)
    {
        var skin = await _skinService.GetByIdAsync(id, cancellationToken);
        if (skin == null)
            return NotFound();

        var rateResult = await _btcRateService.GetCurrentRateAsync(cancellationToken);
        var skinDto = _mapper.Map<SkinDto>(skin);
        skinDto.FinalPriceUsd = _priceCalculator.CalculateFinalPrice(skin.BasePriceUsd, rateResult.Rate);

        return Ok(skinDto);
    }

    [HttpPost]
    public async Task<ActionResult<SkinDto>> CreateSkin(
        CreateSkinDto createDto, 
        [FromServices] IValidator<CreateSkinDto> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(createDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        var skin = _mapper.Map<Skin>(createDto);
        var created = await _skinService.CreateAsync(skin, cancellationToken);
        var skinDto = _mapper.Map<SkinDto>(created);

        return CreatedAtAction(nameof(GetSkin), new { id = created.Id }, skinDto);
    }
    

    [HttpPut("{id}")]
    public async Task<ActionResult<SkinDto>> UpdateSkin(
        Guid id, 
        UpdateSkinDto updateDto,
        [FromServices] IValidator<UpdateSkinDto> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(updateDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var updated = await _skinService.UpdateAsync(
            id, 
            updateDto.Name, 
            updateDto.BasePriceUsd, 
            updateDto.IsAvailable, 
            cancellationToken);

        if (updated == null)
            return NotFound();

        var skinDto = _mapper.Map<SkinDto>(updated);
        return Ok(skinDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSkin(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _skinService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}