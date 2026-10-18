using System.ComponentModel.DataAnnotations;

namespace Irrigation.Application.DTOs.Requests;

public sealed record CancelIrrigationDto
{
    [Required(ErrorMessage = "Reason es obligatorio.")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "Reason debe tener entre 3 y 256 caracteres.")]
    public string Reason { get; init; } = string.Empty;
}
