using System.ComponentModel.DataAnnotations;
using Iot.Domain.Entities;

namespace Iot.Application.DTOs.Requests;

public sealed record ResolveAlertDto
{
    [StringLength(DeviceAlert.MaxResolutionNoteLength, ErrorMessage = "ResolutionNote no puede superar los 512 caracteres.")]
    public string? ResolutionNote { get; init; }
}
