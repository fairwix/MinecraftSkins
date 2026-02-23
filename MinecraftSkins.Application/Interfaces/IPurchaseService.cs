using MinecraftSkins.Domain.Entities;

namespace MinecraftSkins.Application.Interfaces;

public interface IPurchaseService
{
    Task<Purchase?> CreatePurchaseAsync(
        Guid skinId,
        string buyerId,
        decimal btcRate,
        string rateSource,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<Purchase?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Purchase?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);
    Task<List<Purchase>> GetPurchasesAsync(string? buyerId, Guid? skinId, DateTime? from, DateTime? to, int skip, int take, CancellationToken cancellationToken);
}