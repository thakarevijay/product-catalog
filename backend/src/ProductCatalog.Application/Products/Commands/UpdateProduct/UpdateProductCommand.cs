namespace ProductCatalog.Application.Products.Commands.UpdateProduct;

using MediatR;
using ProductCatalog.Domain.Enums;

public record UpdateProductCommand(
    int Id,
    string Name,
    string SKU,
    string? Description,
    decimal Price,
    int StockQuantity,
    ProductStatus Status,
    int CategoryId) : IRequest;
