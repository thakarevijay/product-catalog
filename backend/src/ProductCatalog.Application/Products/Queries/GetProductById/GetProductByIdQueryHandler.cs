namespace ProductCatalog.Application.Products.Queries.GetProductById;

using MediatR;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.Queries.GetProducts;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null) return null;

        return new ProductDto(
            product.Id,
            product.Name,
            product.SKU,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.Status.ToString(),
            product.Category.Name,
            product.ImageUrl,
            product.CreatedAt);
    }
}
