namespace MsDelivery.Domain.Enums;

public enum CapacityLevel
{
    Small = 1,
    Medium = 2,
    Large = 3
}

public static class CapacityLevelExtensions
{
    public static string ToCode(this CapacityLevel capacity) => capacity switch
    {
        CapacityLevel.Small => "SMALL",
        CapacityLevel.Medium => "MEDIUM",
        CapacityLevel.Large => "LARGE",
        _ => throw new ArgumentOutOfRangeException(nameof(capacity), capacity, null)
    };

    public static CapacityLevel FromCode(string code) => code.Trim().ToUpperInvariant() switch
    {
        "SMALL" => CapacityLevel.Small,
        "MEDIUM" => CapacityLevel.Medium,
        "LARGE" => CapacityLevel.Large,
        _ => throw new ArgumentException(
            $"Capacidad inválida: '{code}'. Valores permitidos: SMALL, MEDIUM, LARGE.")
    };
}
