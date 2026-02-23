using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinecraftSkins.Application.Exceptions;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Domain.Entities;
using Npgsql;

namespace MinecraftSkins.Application.Services;


public class PurchaseService : IPurchaseService
{
    private readonly IAppDbContext _context;
    private readonly ILogger<PurchaseService> _logger;
    private readonly IPriceCalculator _priceCalculator;

    public PurchaseService(
        IAppDbContext context,
        ILogger<PurchaseService> logger,
        IPriceCalculator priceCalculator)
    {
        _context = context;
        _logger = logger;
        _priceCalculator = priceCalculator;
    }
    
    public async Task<Purchase?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return null;

        return await _context.Purchases
            .Include(p => p.Skin)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<Purchase?> CreatePurchaseAsync(
    Guid skinId,
    string buyerId,
    decimal btcRate,
    string rateSource,
    string? idempotencyKey,
    CancellationToken cancellationToken)
{
    await _context.BeginTransactionAsync(cancellationToken);

    try
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await _context.Purchases
                .Include(p => p.Skin)
                .FirstOrDefaultAsync(
                    p => p.IdempotencyKey == idempotencyKey,
                    cancellationToken);

            if (existing != null)
            {
                await _context.CommitTransactionAsync(cancellationToken);
                return existing;
            }
        }
        
        var skin = await _context.Skins
            .FirstOrDefaultAsync(s => s.Id == skinId, cancellationToken);

        if (skin == null || skin.IsDeleted)
            throw new SkinUnavailableException(skinId, "Skin not found");

        if (!skin.IsAvailable)
            throw new SkinUnavailableException(skinId, "Skin is not available");
        
        skin.UpdatedAtUtc = DateTime.UtcNow;
        
        var finalPrice = _priceCalculator.CalculateFinalPrice(
            skin.BasePriceUsd,
            btcRate);
        
        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            SkinId = skin.Id,
            PriceUsdFinal = finalPrice,
            BtcUsdRate = btcRate,
            RateSource = rateSource,
            PurchasedAtUtc = DateTime.UtcNow,
            BuyerId = buyerId,
            IdempotencyKey = idempotencyKey
        };

        await _context.Purchases.AddAsync(purchase, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);

        await _context.CommitTransactionAsync(cancellationToken);
        
        await _context.Entry(purchase)
            .Reference(p => p.Skin)
            .LoadAsync(cancellationToken);

        return purchase;
    }
    catch (DbUpdateConcurrencyException)
    {
        await _context.RollbackTransactionAsync(cancellationToken);
        throw new SkinUnavailableException(
            skinId,
            "Skin was modified concurrently, please retry");
    }
    catch (DbUpdateException ex) when (IsUniqueIdempotencyViolation(ex))
    {
        await _context.RollbackTransactionAsync(cancellationToken);
        
        var existing = await _context.Purchases
            .Include(p => p.Skin)
            .AsNoTracking()
            .FirstAsync(
                p => p.IdempotencyKey == idempotencyKey,
                cancellationToken);

        return existing;
    }
    catch
    {
        await _context.RollbackTransactionAsync(cancellationToken);
        throw;
    }
}
    private static bool IsUniqueIdempotencyViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException pg
               && pg.SqlState == PostgresErrorCodes.UniqueViolation;
    }

    public async Task<Purchase?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Purchases
            .Include(p => p.Skin)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Purchase>> GetPurchasesAsync(
        string? buyerId,
        Guid? skinId,
        DateTime? from,
        DateTime? to,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = _context.Purchases
            .Include(p => p.Skin)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(buyerId))
            query = query.Where(p => p.BuyerId == buyerId);

        if (skinId.HasValue)
            query = query.Where(p => p.SkinId == skinId.Value);

        if (from.HasValue)
            query = query.Where(p => p.PurchasedAtUtc >= from.Value);

        if (to.HasValue)
            query = query.Where(p => p.PurchasedAtUtc <= to.Value);

        return await query
            .OrderByDescending(p => p.PurchasedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}