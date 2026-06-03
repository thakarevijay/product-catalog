namespace ProductCatalog.Application.Products.Queries.GetProductById;

using MediatR;
using ProductCatalog.Application.Products.Queries.GetProducts;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
