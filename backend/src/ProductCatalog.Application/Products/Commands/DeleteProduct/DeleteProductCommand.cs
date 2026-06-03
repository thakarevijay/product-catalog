namespace ProductCatalog.Application.Products.Commands.DeleteProduct;

using MediatR;

public record DeleteProductCommand(int Id) : IRequest;
