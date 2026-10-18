using MsInventory.Domain.Entities;
using MsInventory.Domain.Enums;

namespace MsInventory.Domain.Abstractions;

public interface IInventoryRepository
{
    Task<ProductPlant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductPlant?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductPlant>> SearchAsync(string? name, string? category, int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductPlant>> GetBelowMinimumAsync(CancellationToken cancellationToken = default);
    Task AddEntryAsync(InventoryEntry entry, CancellationToken cancellationToken = default);
    Task AddMermaAsync(MermaRecord merma, CancellationToken cancellationToken = default);
    Task<MermaRecord?> GetMermaByIdAsync(Guid mermaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MermaRecord>> GetMermasAsync(Guid? productId, ManagerReviewStatus? status, int skip, int take, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
