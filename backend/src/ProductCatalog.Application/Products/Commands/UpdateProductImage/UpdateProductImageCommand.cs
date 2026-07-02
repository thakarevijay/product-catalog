namespace ProductCatalog.Application.Products.Commands.UpdateProductImage;

using MediatR;

public record UpdateProductImageCommand(int Id, string ImageUrl) : IRequest;
