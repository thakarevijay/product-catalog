namespace ProductCatalog.Application.Products.Queries.GetProducts;

using MediatR;
using ProductCatalog.Application.Common.Models;

public record GetProductsQuery(int Page = 1, int PageSize = 10, string? Search = null)
    : IRequest<PaginatedResult<ProductDto>>;

public record ProductDto(
    int Id,
    string Name,
    string SKU,
    string? Description,
    decimal Price,
    int StockQuantity,
    string Status,
    string CategoryName,
    DateTime CreatedAt);
