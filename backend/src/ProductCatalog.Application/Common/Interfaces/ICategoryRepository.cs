namespace ProductCatalog.Application.Common.Interfaces;

using ProductCatalog.Domain.Entities;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default);
}
