using MsDelivery.Application.DTOs;

namespace MsDelivery.Application.Interfaces;

public interface IDeliveryService
{
    Task<DeliveryOrderDto> CreateOrderAsync(CreateDeliveryOrderRequest request, CancellationToken cancellationToken = default);
    Task<DeliveryOrderDto?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<DeliveryOrderPageDto> GetOrdersAsync(DeliveryOrderQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeliveryOrderDto>> GetOrdersByDriverAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<DeliveryOrderDto> UpdateStatusAsync(Guid orderId, UpdateDeliveryOrderStatusCommand command, CancellationToken cancellationToken = default);
    Task<DeliveryOrderDto> ReassignOrderAsync(Guid orderId, Guid driverId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DriverDto>> GetDriversAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task<DriverDto?> GetDriverAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<DriverDto> CreateDriverAsync(CreateDriverRequest request, CancellationToken cancellationToken = default);
    Task<DriverDto?> UpdateDriverStatusAsync(Guid driverId, bool isActive, CancellationToken cancellationToken = default);
}
