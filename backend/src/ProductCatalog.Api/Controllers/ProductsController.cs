namespace ProductCatalog.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.Commands.CreateProduct;
using ProductCatalog.Application.Products.Commands.DeleteProduct;
using ProductCatalog.Application.Products.Commands.UpdateProduct;
using ProductCatalog.Application.Products.Commands.UpdateProductImage;
using ProductCatalog.Application.Products.Queries.GetProductById;
using ProductCatalog.Application.Products.Queries.GetProducts;
using ProductCatalog.Domain.Enums;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IBlobStorageService _blobStorage;

    public ProductsController(IMediator mediator, IBlobStorageService blobStorage)
    {
        _mediator = mediator;
        _blobStorage = blobStorage;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetProductsQuery(page, pageSize, search), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        // Parse status string to enum
        if (!Enum.TryParse<ProductStatus>(request.Status, out var status))
            return BadRequest($"Invalid status value: {request.Status}");

        var command = new UpdateProductCommand(
            id, request.Name, request.SKU, request.Description,
            request.Price, request.StockQuantity, status, request.CategoryId);

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/image")]
    public async Task<IActionResult> UploadImage(int id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest("Only JPEG, PNG and WebP images are allowed");

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("File size must be less than 5MB");

        var product = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        if (product is null) return NotFound();

        using var stream = file.OpenReadStream();
        var imageUrl = await _blobStorage.UploadImageAsync(
            stream, file.FileName, file.ContentType, cancellationToken);

        await _mediator.Send(new UpdateProductImageCommand(id, imageUrl), cancellationToken);

        return Ok(new { imageUrl });
    }
}

public record UpdateProductRequest(
    string Name,
    string SKU,
    string? Description,
    decimal Price,
    int StockQuantity,
    string Status,
    int CategoryId);
