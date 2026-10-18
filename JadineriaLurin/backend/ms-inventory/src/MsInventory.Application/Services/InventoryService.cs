using MsInventory.Application.Abstractions;
using MsInventory.Application.DTOs;
using MsInventory.Application.Interfaces;
using MsInventory.Domain.Abstractions;
using MsInventory.Domain.Entities;
using MsInventory.Domain.Enums;
using MsInventory.Domain.Events;
using MsInventory.Domain.Exceptions;

namespace MsInventory.Application.Services;

public sealed class InventoryService : IInventoryService
{
    private const string EvidenceFolder = "evidence";
    private const int MaxPageSize = 100;

    private readonly IInventoryRepository _repository;
    private readonly IFileStorageService _fileStorage;
    private readonly ILowStockNotifier _lowStockNotifier;

    public InventoryService(
        IInventoryRepository repository,
        IFileStorageService fileStorage,
        ILowStockNotifier lowStockNotifier)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _lowStockNotifier = lowStockNotifier;
    }

    public async Task<IReadOnlyList<ProductDto>> SearchProductsAsync(
        ProductSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > MaxPageSize ? 20 : query.PageSize;
        var skip = (page - 1) * pageSize;

        var products = await _repository.SearchAsync(
            Normalize(query.Name),
            Normalize(query.Category),
            skip,
            pageSize,
            cancellationToken);

        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(productId, cancellationToken);
        return product is null ? null : MapToDto(product);
    }

    public async Task<IReadOnlyList<InventoryEntryDto>> RegisterEntryBatchAsync(
        RegisterEntryBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Supplier))
            throw new DomainException("El proveedor es obligatorio.");

        if (request.Items is null || request.Items.Count == 0)
            throw new DomainException("El lote debe contener al menos un item.");

        var entryDate = request.EntryDate?.ToUniversalTime() ?? DateTime.UtcNow;
        var entries = new List<InventoryEntry>();

        foreach (var item in request.Items)
        {
            var product = await ResolveProductAsync(item.ProductId, item.Sku, cancellationToken);

            if (item.Quantity <= 0)
                throw new DomainException($"La cantidad para '{product.Sku}' debe ser mayor a cero.");

            if (request.ExpirationDate.HasValue && request.ExpirationDate.Value.Date < entryDate.Date)
                throw new DomainException($"La fecha de vencimiento es anterior a la fecha de ingreso para '{product.Sku}'.");

            product.AddStock(item.Quantity);

            entries.Add(new InventoryEntry
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Product = product,
                Quantity = item.Quantity,
                Supplier = request.Supplier.Trim(),
                EntryDate = entryDate,
                ExpirationDate = request.ExpirationDate?.ToUniversalTime(),
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var entry in entries)
            await _repository.AddEntryAsync(entry, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        return entries.Select(e => new InventoryEntryDto(
            e.Id,
            e.ProductId,
            e.Product!.Sku,
            e.Product.CommonName,
            e.Quantity,
            e.Supplier,
            e.EntryDate,
            e.ExpirationDate)).ToList();
    }

    public async Task<MermaRecordDto> RegisterMermaAsync(
        RegisterMermaCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.QuantityLost <= 0)
            throw new DomainException("La cantidad perdida debe ser mayor a cero.");

        if (string.IsNullOrWhiteSpace(command.Reason))
            throw new DomainException("El motivo de la merma es obligatorio.");

        var product = await _repository.GetByIdAsync(command.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(command.ProductId);

        string? photoUrl = null;
        if (command.EvidencePhotoStream is not null)
        {
            var fileName = string.IsNullOrWhiteSpace(command.EvidencePhotoFileName)
                ? $"{Guid.NewGuid()}.jpg"
                : Path.GetFileName(command.EvidencePhotoFileName);

            photoUrl = await _fileStorage.SaveAsync(command.EvidencePhotoStream, fileName, EvidenceFolder, cancellationToken);
        }

        product.DeductStock(command.QuantityLost);

        var merma = new MermaRecord
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            QuantityLost = command.QuantityLost,
            Reason = command.Reason.Trim(),
            EvidencePhotoUrl = photoUrl,
            ManagerReviewStatus = ManagerReviewStatus.PendingReview,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddMermaAsync(merma, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await RaiseLowStockEventIfBelowMinimumAsync(product, cancellationToken);

        return MapMermaToDto(merma, product);
    }

    public async Task<IReadOnlyList<MermaRecordDto>> GetMermasAsync(
        Guid? productId,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var parsedStatus = string.IsNullOrWhiteSpace(status)
            ? (ManagerReviewStatus?)null
            : ManagerReviewStatusExtensions.FromCode(status);

        var currentPage = page < 1 ? 1 : page;
        var currentPageSize = pageSize is < 1 or > MaxPageSize ? 20 : pageSize;

        var mermas = await _repository.GetMermasAsync(
            productId,
            parsedStatus,
            (currentPage - 1) * currentPageSize,
            currentPageSize,
            cancellationToken);

        return mermas
            .Where(m => m.Product is not null)
            .Select(m => MapMermaToDto(m, m.Product!))
            .ToList();
    }

    public async Task<MermaRecordDto?> GetMermaAsync(Guid mermaId, CancellationToken cancellationToken = default)
    {
        var merma = await _repository.GetMermaByIdAsync(mermaId, cancellationToken);
        return merma?.Product is null ? null : MapMermaToDto(merma, merma.Product);
    }

    public async Task<IReadOnlyList<ShoppingListItemDto>> GetShoppingListAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetBelowMinimumAsync(cancellationToken);

        return products
            .Select(p => new ShoppingListItemDto(
                p.Id,
                p.Sku,
                p.CommonName,
                p.Category,
                p.ImageUrl,
                p.StockAvailable,
                p.StockMinimum,
                Math.Round(p.StockMinimum - p.StockAvailable, 2)))
            .ToList();
    }

    public async Task<DeductStockResultDto> DeductStockForSaleAsync(
        DeductStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ProductNotFoundException(request.ProductId);

        product.DeductStock(request.Quantity);
        await _repository.SaveChangesAsync(cancellationToken);

        var alertTriggered = await RaiseLowStockEventIfBelowMinimumAsync(product, cancellationToken);

        return new DeductStockResultDto(
            product.Id,
            product.Sku,
            product.CommonName,
            product.StockAvailable,
            product.StockMinimum,
            alertTriggered);
    }

    public async Task<MermaRecordDto> ReviewMermaAsync(
        Guid mermaId,
        ReviewMermaRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var newStatus = request.ParseStatus();
        if (newStatus == ManagerReviewStatus.PendingReview)
            throw new DomainException("Una merma en revisión no puede quedar en estado PENDING_REVIEW. Use APPROVED o REJECTED.");

        var merma = await _repository.GetMermaByIdAsync(mermaId, cancellationToken)
            ?? throw new DomainException($"No existe una merma con id '{mermaId}'.");

        if (newStatus == ManagerReviewStatus.Approved)
            merma.Approve();
        else
            merma.ManagerReviewStatus = ManagerReviewStatus.Rejected;

        await _repository.SaveChangesAsync(cancellationToken);

        return MapMermaToDto(merma, merma.Product!);
    }

    private async Task<ProductPlant> ResolveProductAsync(Guid? productId, string? sku, CancellationToken cancellationToken)
    {
        if (productId.HasValue && productId.Value != Guid.Empty)
        {
            return await _repository.GetByIdAsync(productId.Value, cancellationToken)
                ?? throw new ProductNotFoundException(productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(sku))
        {
            return await _repository.GetBySkuAsync(sku.Trim(), cancellationToken)
                ?? throw new ProductNotFoundException(sku.Trim());
        }

        throw new DomainException("Cada item del lote debe indicar ProductId o Sku.");
    }

    private async Task<bool> RaiseLowStockEventIfBelowMinimumAsync(ProductPlant product, CancellationToken cancellationToken)
    {
        if (!product.IsBelowMinimum)
            return false;

        var domainEvent = new LowStockDetectedEvent(
            product.Id,
            product.Sku,
            product.CommonName,
            product.Category,
            product.StockAvailable,
            product.StockMinimum);

        await _lowStockNotifier.NotifyAsync(domainEvent, cancellationToken);
        return true;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ProductDto MapToDto(ProductPlant product) => new(
        product.Id,
        product.Sku,
        product.CommonName,
        product.ImageUrl,
        product.Category,
        product.StockAvailable,
        product.StockMinimum,
        product.IsBelowMinimum,
        product.UpdatedAt);

    private static MermaRecordDto MapMermaToDto(MermaRecord merma, ProductPlant product) => new(
        merma.Id,
        merma.ProductId,
        product.Sku,
        product.CommonName,
        merma.QuantityLost,
        merma.Reason,
        merma.EvidencePhotoUrl,
        merma.ManagerReviewStatus.ToCode(),
        merma.CreatedAt);
}
