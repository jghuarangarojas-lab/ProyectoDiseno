using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MsInventory.Application.DTOs;
using MsInventory.Application.Interfaces;

namespace MsInventory.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Produces("application/json")]
public sealed class InventoryController : ControllerBase
{
    private const long MaxEvidencePhotoBytes = 10 * 1024 * 1024;

    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("products")]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchQuery query, CancellationToken cancellationToken)
    {
        var products = await _inventoryService.SearchProductsAsync(query, cancellationToken);
        return Ok(products);
    }

    [HttpGet("products/{productId:guid}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _inventoryService.GetProductAsync(productId, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost("entries")]
    [ProducesResponseType<IReadOnlyList<InventoryEntryDto>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterEntryBatch([FromBody] RegisterEntryBatchRequest request, CancellationToken cancellationToken)
    {
        var entries = await _inventoryService.RegisterEntryBatchAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, entries);
    }

    [HttpPost("mermas")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxEvidencePhotoBytes)]
    [ProducesResponseType<MermaRecordDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterMerma([FromForm] RegisterMermaForm form, CancellationToken cancellationToken)
    {
        if (form.EvidencePhoto is null || form.EvidencePhoto.Length == 0)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Foto de evidencia requerida",
                Detail = "Debe adjuntar una foto de evidencia (jpg, png, webp o gif) de hasta 10 MB."
            });

        if (form.EvidencePhoto.Length > MaxEvidencePhotoBytes)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Foto demasiado grande",
                Detail = "La foto de evidencia no puede superar los 10 MB."
            });

        await using var photoStream = new MemoryStream();
        await form.EvidencePhoto.CopyToAsync(photoStream, cancellationToken);
        photoStream.Position = 0;

        var merma = await _inventoryService.RegisterMermaAsync(new RegisterMermaCommand
        {
            ProductId = form.ProductId,
            QuantityLost = form.QuantityLost,
            Reason = form.Reason,
            EvidencePhotoStream = photoStream,
            EvidencePhotoFileName = form.EvidencePhoto.FileName
        }, cancellationToken);

        return CreatedAtAction(nameof(GetMerma), new { mermaId = merma.Id }, merma);
    }

    [HttpGet("mermas")]
    [ProducesResponseType<IReadOnlyList<MermaRecordDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMermas(
        [FromQuery] Guid? productId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var mermas = await _inventoryService.GetMermasAsync(productId, status, page, pageSize, cancellationToken);
        return Ok(mermas);
    }

    [HttpGet("mermas/{mermaId:guid}")]
    [ProducesResponseType<MermaRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerma(Guid mermaId, CancellationToken cancellationToken)
    {
        var merma = await _inventoryService.GetMermaAsync(mermaId, cancellationToken);
        return merma is null ? NotFound() : Ok(merma);
    }

    [HttpPost("mermas/{mermaId:guid}/review")]
    [ProducesResponseType<MermaRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReviewMerma(Guid mermaId, [FromBody] ReviewMermaRequest request, CancellationToken cancellationToken)
    {
        var merma = await _inventoryService.ReviewMermaAsync(mermaId, request, cancellationToken);
        return Ok(merma);
    }

    [HttpPost("stock/deduct")]
    [ProducesResponseType<DeductStockResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeductStock([FromBody] DeductStockRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.DeductStockForSaleAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("shopping-list")]
    [ProducesResponseType<IReadOnlyList<ShoppingListItemDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShoppingList(CancellationToken cancellationToken)
    {
        var shoppingList = await _inventoryService.GetShoppingListAsync(cancellationToken);
        return Ok(shoppingList);
    }
}

public sealed class RegisterMermaForm
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public decimal QuantityLost { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = null!;

    [Required]
    public IFormFile EvidencePhoto { get; set; } = null!;
}
