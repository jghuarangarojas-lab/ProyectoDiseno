using Iot.Application.DTOs.Requests;
using Iot.Application.DTOs.Responses;
using Iot.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Iot.Api.Controllers;

[ApiController]
[Route("api/devices")]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<IotDeviceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var devices = await _deviceService.GetAllAsync(cancellationToken);
        return Ok(devices);
    }

    [HttpPost]
    [ProducesResponseType(typeof(IotDeviceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] CreateDeviceDto dto, CancellationToken cancellationToken)
    {
        var device = await _deviceService.RegisterAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetByCode), new { deviceCode = device.DeviceCode }, device);
    }

    [HttpGet("{deviceCode}")]
    [ProducesResponseType(typeof(IotDeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string deviceCode, CancellationToken cancellationToken)
    {
        var device = await _deviceService.GetByCodeAsync(deviceCode, cancellationToken);
        return Ok(device);
    }

    [HttpPut("{deviceCode}/relocate")]
    [ProducesResponseType(typeof(IotDeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Relocate(
        string deviceCode,
        [FromBody] RelocateSensorDto dto,
        CancellationToken cancellationToken)
    {
        var device = await _deviceService.RelocateAsync(deviceCode, dto, cancellationToken);
        return Ok(device);
    }

    [HttpGet("{deviceCode}/thresholds")]
    [ProducesResponseType(typeof(BotanicalThresholdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetThreshold(string deviceCode, CancellationToken cancellationToken)
    {
        var threshold = await _deviceService.GetThresholdAsync(deviceCode, cancellationToken);
        return Ok(threshold);
    }

    [HttpPut("{deviceCode}/thresholds")]
    [ProducesResponseType(typeof(BotanicalThresholdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateThreshold(
        string deviceCode,
        [FromBody] UpdateThresholdDto dto,
        CancellationToken cancellationToken)
    {
        var threshold = await _deviceService.UpdateThresholdAsync(deviceCode, dto, cancellationToken);
        return Ok(threshold);
    }

    [HttpGet("{deviceCode}/telemetry")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TelemetryRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTelemetry(
        string deviceCode,
        [FromQuery] int limit = IDeviceService.DefaultTelemetryLimit,
        CancellationToken cancellationToken = default)
    {
        var records = await _deviceService.GetTelemetryAsync(deviceCode, limit, cancellationToken);
        return Ok(records);
    }
}
