namespace ProductCatalog.Unit.Tests.Products.Validators;

using FluentAssertions;
using NSubstitute;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.Commands.CreateProduct;

public class CreateProductCommandValidatorTests
{
    private readonly IProductRepository _repository;
    private readonly CreateProductCommandValidator _validator;

    public CreateProductCommandValidatorTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _validator = new CreateProductCommandValidator(_repository);

        _repository
            .ExistsBySkuAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Validate_ValidCommand_ShouldPassValidation()
    {
        var command = new CreateProductCommand("Valid Name", "SKU-001", null, 100m, 5, 1);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Validate_EmptyName_ShouldFailValidation(string name)
    {
        var command = new CreateProductCommand(name, "SKU-001", null, 100m, 5, 1);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_InvalidPrice_ShouldFailValidation(decimal price)
    {
        var command = new CreateProductCommand("Valid Name", "SKU-001", null, price, 5, 1);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task Validate_NegativeStockQuantity_ShouldFailValidation()
    {
        var command = new CreateProductCommand("Valid Name", "SKU-001", null, 100m, -1, 1);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StockQuantity");
    }

    [Fact]
    public async Task Validate_DuplicateSKU_ShouldFailValidation()
    {
        _repository
            .ExistsBySkuAsync("DUPLICATE-SKU", Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateProductCommand("Valid Name", "DUPLICATE-SKU", null, 100m, 5, 1);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SKU");
    }

    [Fact]
    public async Task Validate_InvalidCategoryId_ShouldFailValidation()
    {
        var command = new CreateProductCommand("Valid Name", "SKU-001", null, 100m, 5, 0);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
    }
}
