namespace ProductCatalog.Application.Products.Commands.CreateProduct;

using FluentValidation;
using ProductCatalog.Application.Common.Interfaces;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandValidator(IProductRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
            .MustAsync(BeUniqueSku).WithMessage("SKU already exists");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");
    }

    private async Task<bool> BeUniqueSku(string sku, CancellationToken cancellationToken)
        => !await _repository.ExistsBySkuAsync(sku, cancellationToken: cancellationToken);
}
