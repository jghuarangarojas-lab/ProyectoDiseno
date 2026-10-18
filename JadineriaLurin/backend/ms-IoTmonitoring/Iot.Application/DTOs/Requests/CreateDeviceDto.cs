using System.ComponentModel.DataAnnotations;
using Iot.Domain.Entities;

namespace Iot.Application.DTOs.Requests;

public sealed record CreateDeviceDto
{
    [Required(ErrorMessage = "DeviceCode es obligatorio.")]
    [StringLength(IotDevice.MaxDeviceCodeLength, MinimumLength = 1, ErrorMessage = "DeviceCode debe tener entre 1 y 64 caracteres.")]
    public string DeviceCode { get; init; } = string.Empty;

    [Required(ErrorMessage = "MacAddress es obligatoria.")]
    [RegularExpression("^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$", ErrorMessage = "MacAddress debe tener el formato XX:XX:XX:XX:XX:XX.")]
    public string MacAddress { get; init; } = string.Empty;

    [Required(ErrorMessage = "CurrentSectorArea es obligatorio.")]
    [StringLength(IotDevice.MaxSectorAreaLength, MinimumLength = 1, ErrorMessage = "CurrentSectorArea debe tener entre 1 y 32 caracteres.")]
    public string CurrentSectorArea { get; init; } = string.Empty;
}
