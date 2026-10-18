using Microsoft.AspNetCore.Mvc;
using MsDelivery.Application.DTOs;
using MsDelivery.Application.Interfaces;

namespace MsDelivery.Api.Controllers;

[ApiController]
[Route("api/delivery/drivers")]
[Produces("application/json")]
public sealed class DriversController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;

    public DriversController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DriverDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDrivers([FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var drivers = await _deliveryService.GetDriversAsync(isActive, cancellationToken);
        return Ok(drivers);
    }

    [HttpGet("{driverId:guid}")]
    [ProducesResponseType<DriverDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDriver(Guid driverId, CancellationToken cancellationToken)
    {
        var driver = await _deliveryService.GetDriverAsync(driverId, cancellationToken);
        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpPost]
    [ProducesResponseType<DriverDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDriver([FromBody] CreateDriverRequest request, CancellationToken cancellationToken)
    {
        var driver = await _deliveryService.CreateDriverAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDriver), new { driverId = driver.Id }, driver);
    }

    [HttpPut("{driverId:guid}/status")]
    [ProducesResponseType<DriverDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDriverStatus(
        Guid driverId,
        [FromBody] UpdateDriverStatusRequest request,
        CancellationToken cancellationToken)
    {
        var driver = await _deliveryService.UpdateDriverStatusAsync(driverId, request.IsActive, cancellationToken);
        return driver is null ? NotFound() : Ok(driver);
    }
}
