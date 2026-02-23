using Microsoft.EntityFrameworkCore;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Domain.Entities;

namespace MinecraftSkins.Application.Services;

public interface ISkinService
{
    Task<Skin?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAndAvailableAsync(Guid id, CancellationToken cancellationToken);
    Task<Skin?> GetByIdWithVersionAsync(Guid id, CancellationToken cancellationToken);
    Task<Skin> CreateAsync(Skin skin, CancellationToken cancellationToken);
    Task<Skin?> UpdateAsync(Guid id, string? name, decimal? basePrice, bool? isAvailable, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken); 
    Task<List<Skin>> GetSkinsAsync(
        bool availableOnly, 
        string? search, 
        int skip, 
        int take, 
        CancellationToken cancellationToken);
        
    Task<int> GetSkinsCountAsync(bool availableOnly, string? search, CancellationToken cancellationToken);
}

public class SkinService : ISkinService
{
    private readonly IAppDbContext _context;

    public SkinService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Skin?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Skins
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAndAvailableAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Skins
            .AnyAsync(s => s.Id == id && s.IsAvailable && !s.IsDeleted, cancellationToken);
    }
    
    public async Task<Skin?> GetByIdWithVersionAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Skins
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken); 
    }

    public async Task<Skin> CreateAsync(Skin skin, CancellationToken cancellationToken)
    {
        skin.Id = Guid.NewGuid();
        skin.CreatedAtUtc = DateTime.UtcNow;
        
        await _context.Skins.AddAsync(skin, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return skin;
    }

    public async Task<Skin?> UpdateAsync(Guid id, string? name, decimal? basePrice, bool? isAvailable, CancellationToken cancellationToken)
    {
        var skin = await _context.Skins.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (skin == null) return null;

        if (name != null) skin.Name = name;
        if (basePrice.HasValue) skin.BasePriceUsd = basePrice.Value;
        if (isAvailable.HasValue) skin.IsAvailable = isAvailable.Value;
        
        skin.UpdatedAtUtc = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);
        return skin;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var skin = await _context.Skins.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (skin == null) return false;

        skin.IsDeleted = true;
        skin.DeletedAtUtc = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
    
    public async Task<List<Skin>> GetSkinsAsync(
        bool availableOnly, 
        string? search, 
        int skip, 
        int take, 
        CancellationToken cancellationToken)
    {
        IQueryable<Skin> query = _context.Skins
            .AsNoTracking();

        if (availableOnly)
            query = query.Where(s => s.IsAvailable);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Name.ToLower().Contains(search.ToLower()));
        }

        return await query
            .OrderBy(s => s.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetSkinsCountAsync(
        bool availableOnly, 
        string? search, 
        CancellationToken cancellationToken)
    {
        IQueryable<Skin> query = _context.Skins
            .AsNoTracking();

        if (availableOnly)
            query = query.Where(s => s.IsAvailable);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Name.ToLower().Contains(search.ToLower()));
        }

        return await query.CountAsync(cancellationToken);
    }
}