namespace ProductCatalog.Application.Products.Commands.UpdateProductImage;

using MediatR;
using ProductCatalog.Application.Common.Interfaces;

public class UpdateProductImageCommandHandler : IRequestHandler<UpdateProductImageCommand>
{
    private readonly IProductRepository _repository;

    public UpdateProductImageCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdateProductImageCommand request, CancellationToken cancellationToken)
    {
        await _repository.UpdateImageUrlAsync(request.Id, request.ImageUrl, cancellationToken);
    }
}
