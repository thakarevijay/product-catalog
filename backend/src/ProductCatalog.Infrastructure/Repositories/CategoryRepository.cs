namespace ProductCatalog.Infrastructure.Repositories;

using Dapper;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Domain.Entities;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CategoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Category>(
            "SELECT Id, Name, Description FROM Categories ORDER BY Name");
    }
}
