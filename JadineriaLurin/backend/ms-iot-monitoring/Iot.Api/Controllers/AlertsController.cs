using Iot.Application.DTOs.Requests;
using Iot.Application.DTOs.Responses;
using Iot.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Iot.Api.Controllers;

[ApiController]
[Route("api/alerts")]
[Produces("application/json")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<DeviceAlertDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] string? deviceCode,
        [FromQuery] bool includeResolved = false,
        CancellationToken cancellationToken = default)
    {
        var alerts = await _alertService.GetAlertsAsync(deviceCode, includeResolved, cancellationToken);
        return Ok(alerts);
    }

    [HttpPut("{id:guid}/resolve")]
    [ProducesResponseType(typeof(DeviceAlertDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Resolve(
        Guid id,
        [FromBody] ResolveAlertDto dto,
        CancellationToken cancellationToken)
    {
        var alert = await _alertService.ResolveAlertAsync(id, dto, cancellationToken);
        return Ok(alert);
    }
}
