using Iot.Application.DTOs.Requests;
using Iot.Application.DTOs.Responses;
using Iot.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Iot.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
[Produces("application/json")]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryService _telemetryService;

    public TelemetryController(ITelemetryService telemetryService)
    {
        _telemetryService = telemetryService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IotDeviceDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ingest([FromBody] TelemetryInputDto dto, CancellationToken cancellationToken)
    {
        var device = await _telemetryService.ProcessTelemetryAsync(dto, cancellationToken);
        return Accepted(device);
    }

    [HttpPost("{deviceCode}/unavailable")]
    [ProducesResponseType(typeof(IotDeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkUnavailable(
        string deviceCode,
        [FromBody] MarkUnavailableDto dto,
        CancellationToken cancellationToken)
    {
        var reason = dto.Reason ?? "Desconexión reportada manualmente desde el panel de administración.";
        var device = await _telemetryService.HandleDeviceUnavailableAsync(deviceCode, reason, cancellationToken);

        if (device is null)
        {
            return NotFound(new { message = $"No existe ningún dispositivo IoT registrado con código '{deviceCode}'." });
        }

        return Ok(device);
    }
}
