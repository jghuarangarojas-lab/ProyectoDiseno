using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MsDelivery.Application.DTOs;
using MsDelivery.Application.Interfaces;

namespace MsDelivery.Api.Controllers;

[ApiController]
[Route("api/delivery/orders")]
[Produces("application/json")]
public sealed class DeliveryOrdersController : ControllerBase
{
    private const long MaxFileBytes = 10 * 1024 * 1024;

    private readonly IDeliveryService _deliveryService;

    public DeliveryOrdersController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpPost]
    [ProducesResponseType<DeliveryOrderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateDeliveryOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _deliveryService.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { orderId = order.Id }, order);
    }

    [HttpGet]
    [ProducesResponseType<DeliveryOrderPageDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders([FromQuery] DeliveryOrderQuery query, CancellationToken cancellationToken)
    {
        var page = await _deliveryService.GetOrdersAsync(query, cancellationToken);
        return Ok(page);
    }

    [HttpGet("driver/{driverId:guid}")]
    [ProducesResponseType<IReadOnlyList<DeliveryOrderDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrdersByDriver(Guid driverId, CancellationToken cancellationToken)
    {
        var orders = await _deliveryService.GetOrdersByDriverAsync(driverId, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType<DeliveryOrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _deliveryService.GetOrderAsync(orderId, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPut("{orderId:guid}/status")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileBytes * 2 + 1024 * 1024)]
    [ProducesResponseType<DeliveryOrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStatus(Guid orderId, [FromForm] UpdateDeliveryOrderStatusForm form, CancellationToken cancellationToken)
    {
        if (form.ProofPhoto is not null && form.ProofPhoto.Length > MaxFileBytes)
            return BuildProblem(StatusCodes.Status400BadRequest, "Foto demasiado grande", "La foto de evidencia no puede superar los 10 MB.");

        if (form.DigitalSignature is not null && form.DigitalSignature.Length > MaxFileBytes)
            return BuildProblem(StatusCodes.Status400BadRequest, "Firma demasiado grande", "La firma digital no puede superar los 10 MB.");

        var command = new UpdateDeliveryOrderStatusCommand
        {
            Status = form.Status,
            FailureReason = form.FailureReason
        };

        if (form.ProofPhoto is not null)
        {
            var photoStream = new MemoryStream();
            await form.ProofPhoto.CopyToAsync(photoStream, cancellationToken);
            photoStream.Position = 0;
            command.ProofPhotoStream = photoStream;
            command.ProofPhotoFileName = form.ProofPhoto.FileName;
        }

        if (form.DigitalSignature is not null)
        {
            var signatureStream = new MemoryStream();
            await form.DigitalSignature.CopyToAsync(signatureStream, cancellationToken);
            signatureStream.Position = 0;
            command.DigitalSignatureStream = signatureStream;
            command.DigitalSignatureFileName = form.DigitalSignature.FileName;
        }

        try
        {
            var order = await _deliveryService.UpdateStatusAsync(orderId, command, cancellationToken);
            return Ok(order);
        }
        finally
        {
            if (command.ProofPhotoStream is not null)
                await command.ProofPhotoStream.DisposeAsync();

            if (command.DigitalSignatureStream is not null)
                await command.DigitalSignatureStream.DisposeAsync();
        }
    }

    [HttpPut("{orderId:guid}/driver")]
    [ProducesResponseType<DeliveryOrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReassignOrder(Guid orderId, [FromBody] ReassignOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _deliveryService.ReassignOrderAsync(orderId, request.DriverId, cancellationToken);
        return Ok(order);
    }

    private ObjectResult BuildProblem(int status, string title, string detail) => StatusCode(status, new ProblemDetails
    {
        Status = status,
        Title = title,
        Detail = detail,
        Instance = HttpContext.Request.Path
    });
}

public sealed class UpdateDeliveryOrderStatusForm
{
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = null!;

    [MaxLength(500)]
    public string? FailureReason { get; set; }

    public IFormFile? ProofPhoto { get; set; }

    public IFormFile? DigitalSignature { get; set; }
}
