namespace ProductCatalog.Unit.Tests.Products.Commands;

using FluentAssertions;
using NSubstitute;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.Commands.CreateProduct;
using ProductCatalog.Domain.Entities;

public class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateProductCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddProductAndSave()
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", "SKU-001", null, 100m, 10, 1);

        _repository
            .AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var product = callInfo.Arg<Product>();
                product.Id = 1;
                return product;
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(1);
        await _repository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Name == "Test Product" && p.SKU == "SKU-001"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetCorrectCategoryId()
    {
        // Arrange
        var command = new CreateProductCommand("Test", "SKU-002", null, 100m, 5, 2);

        _repository
            .AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Product>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.CategoryId == 2),
            Arg.Any<CancellationToken>());
    }
}
