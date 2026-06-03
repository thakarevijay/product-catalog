namespace ProductCatalog.Unit.Tests.Products.Queries;

using FluentAssertions;
using NSubstitute;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.Queries.GetProductById;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;

public class GetProductByIdQueryHandlerTests
{
    private readonly IProductRepository _repository;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _handler = new GetProductByIdQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldReturnProductDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            SKU = "SKU-001",
            Price = 500m,
            StockQuantity = 10,
            Status = ProductStatus.Active,
            Category = new Category { Id = 1, Name = "Bags" }
        };

        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(product);

        // Act
        var result = await _handler.Handle(new GetProductByIdQuery(1), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
        result.Price.Should().Be(500m);
        result.CategoryName.Should().Be("Bags");
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_NonExistingProduct_ShouldReturnNull()
    {
        // Arrange
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Product?)null);

        // Act
        var result = await _handler.Handle(new GetProductByIdQuery(99), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
