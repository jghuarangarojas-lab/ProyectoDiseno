using Microsoft.EntityFrameworkCore;
using MsInventory.Domain.Abstractions;
using MsInventory.Domain.Entities;
using MsInventory.Domain.Enums;
using MsInventory.Infrastructure.Persistence;

namespace MsInventory.Infrastructure.Persistence.Repositories;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _dbContext;

    public InventoryRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductPlant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.ProductPlants
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<ProductPlant?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) =>
        _dbContext.ProductPlants
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);

    public async Task<IReadOnlyList<ProductPlant>> SearchAsync(
        string? name,
        string? category,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ProductPlants.AsNoTracking();

        if (!string.IsNullOrEmpty(name))
            query = query.Where(p => EF.Functions.ILike(p.CommonName, $"%{EscapeLikeParameter(name)}%"));

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => EF.Functions.ILike(p.Category, $"%{EscapeLikeParameter(category)}%"));

        return await query
            .OrderBy(p => p.CommonName)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductPlant>> GetBelowMinimumAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.ProductPlants
            .AsNoTracking()
            .Where(p => p.StockAvailable < p.StockMinimum)
            .OrderBy(p => p.StockAvailable / p.StockMinimum)
            .ThenBy(p => p.CommonName)
            .ToListAsync(cancellationToken);

    public Task AddEntryAsync(InventoryEntry entry, CancellationToken cancellationToken = default) =>
        _dbContext.InventoryEntries.AddAsync(entry, cancellationToken).AsTask();

    public Task AddMermaAsync(MermaRecord merma, CancellationToken cancellationToken = default) =>
        _dbContext.MermaRecords.AddAsync(merma, cancellationToken).AsTask();

    public Task<MermaRecord?> GetMermaByIdAsync(Guid mermaId, CancellationToken cancellationToken = default) =>
        _dbContext.MermaRecords
            .Include(m => m.Product)
            .FirstOrDefaultAsync(m => m.Id == mermaId, cancellationToken);

    public async Task<IReadOnlyList<MermaRecord>> GetMermasAsync(
        Guid? productId,
        ManagerReviewStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        IQueryable<MermaRecord> query = _dbContext.MermaRecords
            .AsNoTracking()
            .Include(m => m.Product);

        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);

        if (status.HasValue)
            query = query.Where(m => m.ManagerReviewStatus == status.Value);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    private static string EscapeLikeParameter(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
