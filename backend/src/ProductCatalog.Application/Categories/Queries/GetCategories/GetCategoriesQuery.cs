namespace ProductCatalog.Application.Categories.Queries.GetCategories;

using MediatR;

public record GetCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;

public record CategoryDto(int Id, string Name);
