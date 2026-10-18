using System.ComponentModel.DataAnnotations;
using Iot.Domain.Entities;

namespace Iot.Application.DTOs.Requests;

public sealed record RelocateSensorDto
{
    [Required(ErrorMessage = "CurrentSectorArea es obligatorio.")]
    [StringLength(IotDevice.MaxSectorAreaLength, MinimumLength = 1, ErrorMessage = "CurrentSectorArea debe tener entre 1 y 32 caracteres.")]
    public string CurrentSectorArea { get; init; } = string.Empty;
}
