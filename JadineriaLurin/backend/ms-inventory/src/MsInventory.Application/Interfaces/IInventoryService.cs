using MsInventory.Application.DTOs;

namespace MsInventory.Application.Interfaces;

public interface IInventoryService
{
    Task<IReadOnlyList<ProductDto>> SearchProductsAsync(ProductSearchQuery query, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryEntryDto>> RegisterEntryBatchAsync(RegisterEntryBatchRequest request, CancellationToken cancellationToken = default);
    Task<MermaRecordDto> RegisterMermaAsync(RegisterMermaCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MermaRecordDto>> GetMermasAsync(Guid? productId, string? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<MermaRecordDto?> GetMermaAsync(Guid mermaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShoppingListItemDto>> GetShoppingListAsync(CancellationToken cancellationToken = default);
    Task<DeductStockResultDto> DeductStockForSaleAsync(DeductStockRequest request, CancellationToken cancellationToken = default);
    Task<MermaRecordDto> ReviewMermaAsync(Guid mermaId, ReviewMermaRequest request, CancellationToken cancellationToken = default);
}
