namespace ProductCatalog.Unit.Tests.Products.Commands;

using FluentAssertions;
using NSubstitute;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.Commands.DeleteProduct;
using ProductCatalog.Domain.Entities;

public class DeleteProductCommandHandlerTests
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteProductCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldDeleteAndSave()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", SKU = "SKU-001" };
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(product);

        // Act
        await _handler.Handle(new DeleteProductCommand(1), CancellationToken.None);

        // Assert
        await _repository.Received(1).DeleteAsync(product, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistingProduct_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Product?)null);

        // Act
        var act = async () => await _handler.Handle(
            new DeleteProductCommand(99), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*99*");
    }
}
