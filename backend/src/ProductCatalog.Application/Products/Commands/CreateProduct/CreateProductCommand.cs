namespace ProductCatalog.Application.Products.Commands.CreateProduct;

using MediatR;

public record CreateProductCommand(
    string Name,
    string SKU,
    string? Description,
    decimal Price,
    int StockQuantity,
    int CategoryId) : IRequest<int>;
