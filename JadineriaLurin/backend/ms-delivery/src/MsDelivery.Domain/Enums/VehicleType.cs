namespace MsDelivery.Domain.Enums;

public enum VehicleType
{
    Motorcycle = 0,
    Van = 1,
    Truck = 2
}

public static class VehicleTypeExtensions
{
    public static string ToCode(this VehicleType vehicleType) => vehicleType switch
    {
        VehicleType.Motorcycle => "MOTORCYCLE",
        VehicleType.Van => "VAN",
        VehicleType.Truck => "TRUCK",
        _ => throw new ArgumentOutOfRangeException(nameof(vehicleType), vehicleType, null)
    };

    public static VehicleType FromCode(string code) => code.Trim().ToUpperInvariant() switch
    {
        "MOTORCYCLE" => VehicleType.Motorcycle,
        "VAN" => VehicleType.Van,
        "TRUCK" => VehicleType.Truck,
        _ => throw new ArgumentException(
            $"Tipo de vehículo inválido: '{code}'. Valores permitidos: MOTORCYCLE, VAN, TRUCK.")
    };
}
