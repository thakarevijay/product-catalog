namespace ProductCatalog.Application.Products.Queries.GetProducts;

using MediatR;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Common.Models;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedResult<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(request.Page, request.PageSize, request.Search, cancellationToken);
        var totalCount = await _repository.GetTotalCountAsync(request.Search, cancellationToken);

        var items = products.Select(p => new ProductDto(
            p.Id, p.Name, p.SKU, p.Description,
            p.Price, p.StockQuantity, p.Status.ToString(),
            p.Category.Name, p.CreatedAt));

        return new PaginatedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
