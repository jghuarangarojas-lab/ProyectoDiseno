using System.ComponentModel.DataAnnotations;
using Irrigation.Domain.Entities;

namespace Irrigation.Application.DTOs.Requests;

public sealed record StartIrrigationDto
{
    [Required(ErrorMessage = "DeviceId es obligatorio.")]
    [StringLength(64, MinimumLength = 1, ErrorMessage = "DeviceId debe tener entre 1 y 64 caracteres.")]
    public string DeviceId { get; init; } = string.Empty;

    [Required(ErrorMessage = "SectorArea es obligatorio.")]
    [StringLength(32, MinimumLength = 1, ErrorMessage = "SectorArea debe tener entre 1 y 32 caracteres.")]
    public string SectorArea { get; init; } = string.Empty;

    [RegularExpression("^(AUTOMATIC|MANUAL)$", ErrorMessage = "TriggerType debe ser AUTOMATIC o MANUAL.")]
    public string? TriggerType { get; init; }

    [Range(
        IrrigationAction.MinDurationSeconds,
        IrrigationAction.PumpDurationLimitSeconds,
        ErrorMessage = "RequestedDurationSeconds debe estar entre 1 y 60 segundos.")]
    public int RequestedDurationSeconds { get; init; }
}
