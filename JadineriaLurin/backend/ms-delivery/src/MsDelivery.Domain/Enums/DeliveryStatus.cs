namespace MsDelivery.Domain.Enums;

public enum DeliveryStatus
{
    Pending = 0,
    Dispatched = 1,
    Delivered = 2,
    Failed = 3
}

public static class DeliveryStatusExtensions
{
    public static string ToCode(this DeliveryStatus status) => status switch
    {
        DeliveryStatus.Pending => "PENDING",
        DeliveryStatus.Dispatched => "DISPATCHED",
        DeliveryStatus.Delivered => "DELIVERED",
        DeliveryStatus.Failed => "FAILED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static DeliveryStatus FromCode(string code) => code.Trim().ToUpperInvariant() switch
    {
        "PENDING" => DeliveryStatus.Pending,
        "DISPATCHED" => DeliveryStatus.Dispatched,
        "DELIVERED" => DeliveryStatus.Delivered,
        "FAILED" => DeliveryStatus.Failed,
        _ => throw new ArgumentException(
            $"Estado inválido: '{code}'. Valores permitidos: PENDING, DISPATCHED, DELIVERED, FAILED.")
    };
}
