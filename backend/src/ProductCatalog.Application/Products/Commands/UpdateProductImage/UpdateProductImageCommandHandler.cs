namespace ProductCatalog.Application.Products.Commands.UpdateProductImage;

using MediatR;
using ProductCatalog.Application.Common.Interfaces;

public class UpdateProductImageCommandHandler : IRequestHandler<UpdateProductImageCommand>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductImageCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found");

        product.ImageUrl = request.ImageUrl;
        product.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
