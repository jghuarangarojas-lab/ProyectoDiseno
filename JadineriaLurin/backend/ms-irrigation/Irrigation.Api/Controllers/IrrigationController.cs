using Irrigation.Application.DTOs.Requests;
using Irrigation.Application.DTOs.Responses;
using Irrigation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Irrigation.Api.Controllers;

[ApiController]
[Route("api/irrigation")]
[Produces("application/json")]
public class IrrigationController : ControllerBase
{
    private readonly IIrrigationService _irrigationService;

    public IrrigationController(IIrrigationService irrigationService)
    {
        _irrigationService = irrigationService;
    }

    [HttpPost("start")]
    [ProducesResponseType(typeof(IrrigationActionDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Start([FromBody] StartIrrigationDto dto, CancellationToken cancellationToken)
    {
        var result = await _irrigationService.StartAsync(dto, cancellationToken);
        return AcceptedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(IrrigationActionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelIrrigationDto dto, CancellationToken cancellationToken)
    {
        var result = await _irrigationService.CancelAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/cancellation-reason")]
    [ProducesResponseType(typeof(IrrigationActionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCancellationReason(Guid id, [FromBody] UpdateCancellationReasonDto dto, CancellationToken cancellationToken)
    {
        var result = await _irrigationService.UpdateCancellationReasonAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IrrigationActionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _irrigationService.GetByIdAsync(id, cancellationToken);

        if (result is null)
        {
            return NotFound(new { message = $"No existe ninguna acción de riego con id '{id}'." });
        }

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<IrrigationActionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDevice(
        [FromQuery] string deviceId,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var results = await _irrigationService.GetRecentByDeviceAsync(deviceId, limit, cancellationToken);
        return Ok(results);
    }
}
