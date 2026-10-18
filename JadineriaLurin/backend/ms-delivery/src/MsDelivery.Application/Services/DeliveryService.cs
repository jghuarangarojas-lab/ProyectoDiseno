using Microsoft.Extensions.Logging;
using MsDelivery.Application.Abstractions;
using MsDelivery.Application.DTOs;
using MsDelivery.Application.Interfaces;
using MsDelivery.Domain.Abstractions;
using MsDelivery.Domain.Entities;
using MsDelivery.Domain.Enums;
using MsDelivery.Domain.Exceptions;

namespace MsDelivery.Application.Services;

public sealed class DeliveryService : IDeliveryService
{
    private const int MaxPageSize = 100;
    private const string ProofFolder = "deliveries/proofs";
    private const string SignatureFolder = "deliveries/signatures";
    private const long MaxFileBytes = 10 * 1024 * 1024;

    private readonly IDeliveryRepository _repository;
    private readonly IFileStorageService _fileStorage;
    private readonly IWhatsAppClient _whatsAppClient;
    private readonly ILogger<DeliveryService> _logger;

    public DeliveryService(
        IDeliveryRepository repository,
        IFileStorageService fileStorage,
        IWhatsAppClient whatsAppClient,
        ILogger<DeliveryService> logger)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _whatsAppClient = whatsAppClient;
        _logger = logger;
    }

    public async Task<DeliveryOrderDto> CreateOrderAsync(
        CreateDeliveryOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await _repository.GetOrderBySaleIdAsync(request.SaleId, cancellationToken) is not null)
            throw new DomainException($"Ya existe una orden de entrega registrada para la venta '{request.SaleId}'.");

        if (string.IsNullOrWhiteSpace(request.Address))
            throw new DomainException("La dirección de entrega es obligatoria.");

        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new DomainException("El teléfono de contacto es obligatorio.");

        var order = new DeliveryOrder
        {
            Id = Guid.NewGuid(),
            SaleId = request.SaleId,
            Address = request.Address.Trim(),
            DeliveryDate = request.DeliveryDate,
            DeliveryTime = request.DeliveryTime,
            Phone = request.Phone.Trim(),
            Reference = request.Reference?.Trim() ?? string.Empty,
            RequiredCapacity = CapacityLevelExtensions.FromCode(request.RequiredCapacity),
            Status = DeliveryStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await TryAutoAssignAsync(order, cancellationToken);

        await _repository.AddOrderAsync(order, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Orden {OrderId} (venta {SaleId}) creada con estado {Status}. Repartidor asignado: {DriverId}.",
            order.Id, order.SaleId, order.Status.ToCode(), order.DriverId);

        return MapToDto(order);
    }

    public async Task<DeliveryOrderDto?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetOrderByIdAsync(orderId, includeDriver: true, cancellationToken);
        return order is null ? null : MapToDto(order);
    }

    public async Task<DeliveryOrderPageDto> GetOrdersAsync(
        DeliveryOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        query ??= new DeliveryOrderQuery();

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > MaxPageSize ? 50 : query.PageSize;
        var status = ParseOptionalStatus(query.Status);

        var total = await _repository.CountOrdersAsync(status, query.DriverId, query.DeliveryDate, cancellationToken);
        var items = await _repository.GetOrdersAsync(
            status, query.DriverId, query.DeliveryDate,
            (page - 1) * pageSize, pageSize, cancellationToken);

        return new DeliveryOrderPageDto(items.Select(MapToDto).ToList(), total, page, pageSize);
    }

    public async Task<IReadOnlyList<DeliveryOrderDto>> GetOrdersByDriverAsync(
        Guid driverId,
        CancellationToken cancellationToken = default)
    {
        if (await _repository.GetDriverByIdAsync(driverId, cancellationToken: cancellationToken) is null)
            throw new DriverNotFoundException(driverId);

        var orders = await _repository.GetOrdersAsync(
            driverId: driverId, skip: 0, take: MaxPageSize, cancellationToken: cancellationToken);

        return orders.Select(MapToDto).ToList();
    }

    public async Task<DeliveryOrderDto> UpdateStatusAsync(
        Guid orderId,
        UpdateDeliveryOrderStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var order = await _repository.GetOrderByIdAsync(orderId, includeDriver: true, cancellationToken)
            ?? throw new DeliveryOrderNotFoundException(orderId);

        var targetStatus = DeliveryStatusExtensions.FromCode(command.Status);

        if (order.Status == targetStatus)
            throw new InvalidStatusTransitionException(order.Status, targetStatus);

        if (!order.CanTransitionTo(targetStatus))
            throw new InvalidStatusTransitionException(order.Status, targetStatus);

        if (targetStatus == DeliveryStatus.Failed && string.IsNullOrWhiteSpace(command.FailureReason))
            throw new DomainException("El motivo de falla (FailureReason) es obligatorio para marcar la orden como FAILED.");

        string? proofPhotoUrl = null;
        string? signatureUrl = null;

        if (command.ProofPhotoStream is not null)
            proofPhotoUrl = await SaveFileAsync(command.ProofPhotoStream, command.ProofPhotoFileName, ProofFolder, "foto de evidencia", cancellationToken);

        if (command.DigitalSignatureStream is not null)
            signatureUrl = await SaveFileAsync(command.DigitalSignatureStream, command.DigitalSignatureFileName, SignatureFolder, "firma digital", cancellationToken);

        switch (targetStatus)
        {
            case DeliveryStatus.Delivered:
                order.MarkDelivered(proofPhotoUrl, signatureUrl);
                break;

            case DeliveryStatus.Failed:
                order.MarkFailed(command.FailureReason!);
                break;

            case DeliveryStatus.Dispatched:
                order.MarkDispatched();
                break;

            default:
                throw new InvalidStatusTransitionException(order.Status, targetStatus);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        await NotifyStatusChangeAsync(order, targetStatus, cancellationToken);

        _logger.LogInformation(
            "Orden {OrderId} pasó a {Status}. Repartidor: {DriverId}.",
            order.Id, targetStatus.ToCode(), order.DriverId);

        return MapToDto(order);
    }

    public async Task<DeliveryOrderDto> ReassignOrderAsync(
        Guid orderId,
        Guid driverId,
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetOrderByIdAsync(orderId, includeDriver: true, cancellationToken)
            ?? throw new DeliveryOrderNotFoundException(orderId);

        var driver = await _repository.GetDriverByIdAsync(driverId, cancellationToken: cancellationToken)
            ?? throw new DriverNotFoundException(driverId);

        order.ChangeDriver(driver);

        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Reasignación manual: orden {OrderId} → repartidor {DriverName} ({DriverId}).",
            order.Id, driver.Name, driver.Id);

        return MapToDto(order);
    }

    public async Task<IReadOnlyList<DriverDto>> GetDriversAsync(
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var drivers = await _repository.GetDriversAsync(isActive, cancellationToken);
        var activeOrders = await _repository.GetActiveOrdersByDriverIdsAsync(
            drivers.Select(d => d.Id).ToList(), cancellationToken);

        var loadByDriver = activeOrders
            .Where(o => o.DriverId.HasValue)
            .GroupBy(o => o.DriverId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return drivers
            .Select(d => MapToDto(d, loadByDriver.GetValueOrDefault(d.Id)))
            .ToList();
    }

    public async Task<DriverDto?> GetDriverAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        var driver = await _repository.GetDriverByIdAsync(driverId, includeOrders: true, cancellationToken);
        if (driver is null)
            return null;

        var activeOrders = driver.Orders.Count(o => !DeliveryOrder.IsTerminal(o.Status));
        return MapToDto(driver, activeOrders);
    }

    public async Task<DriverDto> CreateDriverAsync(
        CreateDriverRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("El nombre del repartidor es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new DomainException("El teléfono del repartidor es obligatorio.");

        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim(),
            VehicleType = VehicleTypeExtensions.FromCode(request.VehicleType),
            MaxCapacity = CapacityLevelExtensions.FromCode(request.MaxCapacity),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddDriverAsync(driver, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapToDto(driver, 0);
    }

    public async Task<DriverDto?> UpdateDriverStatusAsync(
        Guid driverId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var driver = await _repository.GetDriverByIdAsync(driverId, includeOrders: true, cancellationToken);
        if (driver is null)
            return null;

        if (isActive)
            driver.Activate();
        else
            driver.Deactivate();

        await _repository.SaveChangesAsync(cancellationToken);

        var activeOrders = driver.Orders.Count(o => !DeliveryOrder.IsTerminal(o.Status));
        return MapToDto(driver, activeOrders);
    }

    private async Task TryAutoAssignAsync(DeliveryOrder order, CancellationToken cancellationToken)
    {
        var drivers = await _repository.GetDriversAsync(isActive: true, cancellationToken);
        var candidates = drivers.Where(d => d.Supports(order.RequiredCapacity)).ToList();

        if (candidates.Count == 0)
        {
            _logger.LogWarning(
                "Sin repartidores activos con capacidad {Capacity} para la orden {OrderId}; queda en PENDING sin asignar.",
                order.RequiredCapacity.ToCode(), order.Id);
            return;
        }

        var activeOrders = await _repository.GetActiveOrdersByDriverIdsAsync(
            candidates.Select(d => d.Id).ToList(), cancellationToken);

        var loadByDriver = activeOrders
            .Where(o => o.DriverId.HasValue)
            .GroupBy(o => o.DriverId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var bestDriver = candidates
            .OrderBy(d => loadByDriver.GetValueOrDefault(d.Id))
            .ThenBy(d => d.MaxCapacity)
            .ThenBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .First();

        order.AssignDriver(bestDriver);

        _logger.LogInformation(
            "Auto-asignación: orden {OrderId} → repartidor {DriverName} ({DriverId}), carga activa {Load}.",
            order.Id, bestDriver.Name, bestDriver.Id, loadByDriver.GetValueOrDefault(bestDriver.Id));
    }

    private async Task NotifyStatusChangeAsync(
        DeliveryOrder order,
        DeliveryStatus status,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(order.Phone))
            return;

        var message = status switch
        {
            DeliveryStatus.Dispatched =>
                $"Hola, tu pedido de Jardinería Lurín (ref. {order.Reference}) está en camino. " +
                $"Llegará el {order.DeliveryDate:dd/MM/yyyy} a las {order.DeliveryTime:HH\\:mm}. " +
                $"Repartidor: {order.Driver?.Name ?? "asignado"}.",
            DeliveryStatus.Delivered =>
                $"Hola, tu pedido de Jardinería Lurín (ref. {order.Reference}) fue entregado. " +
                "¡Gracias por tu compra!",
            DeliveryStatus.Failed =>
                $"Hola, no pudimos entregar tu pedido de Jardinería Lurín (ref. {order.Reference}). " +
                $"Motivo: {order.FailureReason}. Contáctanos para reprogramar la entrega.",
            _ => null
        };

        if (message is null)
            return;

        try
        {
            var sent = await _whatsAppClient.SendTextAsync(order.Phone, message, cancellationToken);
            if (!sent)
                _logger.LogWarning("WhatsApp no confirmó el envío de la notificación de la orden {OrderId}.", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al notificar por WhatsApp el estado {Status} de la orden {OrderId}.",
                status.ToCode(), order.Id);
        }
    }

    private async Task<string> SaveFileAsync(
        Stream content,
        string? fileName,
        string folder,
        string fileDescription,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException($"El {fileDescription} debe incluir un nombre de archivo.");

        if (content.CanSeek && content.Length > MaxFileBytes)
            throw new DomainException($"El {fileDescription} supera el tamaño máximo de 10 MB.");

        return await _fileStorage.SaveAsync(content, fileName, folder, cancellationToken);
    }

    private static DeliveryStatus? ParseOptionalStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        return DeliveryStatusExtensions.FromCode(status);
    }

    private static DeliveryOrderDto MapToDto(DeliveryOrder order) => new(
        order.Id,
        order.SaleId,
        order.Address,
        order.DeliveryDate,
        order.DeliveryTime,
        order.Phone,
        order.Reference,
        order.RequiredCapacity.ToCode(),
        order.Status.ToCode(),
        order.DriverId,
        order.Driver?.Name,
        order.Driver?.VehicleType.ToCode(),
        order.FailureReason,
        order.ProofPhotoUrl,
        order.DigitalSignatureUrl,
        order.CreatedAt,
        order.UpdatedAt);

    private static DriverDto MapToDto(Driver driver, int activeOrders) => new(
        driver.Id,
        driver.Name,
        driver.Phone,
        driver.VehicleType.ToCode(),
        driver.MaxCapacity.ToCode(),
        driver.IsActive,
        activeOrders,
        driver.CreatedAt,
        driver.UpdatedAt);
}
