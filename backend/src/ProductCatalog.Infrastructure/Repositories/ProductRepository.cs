namespace ProductCatalog.Infrastructure.Repositories;

using Dapper;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Infrastructure.Persistence;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(ApplicationDbContext context, IDbConnectionFactory connectionFactory)
    {
        _context = context;
        _connectionFactory = connectionFactory;
    }

    // ─── READ SIDE — Dapper ───────────────────────────────────────────────

    public async Task<IEnumerable<Product>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var sql = """
            SELECT
                p.Id, p.Name, p.SKU, p.Description, p.Price,
                p.StockQuantity, p.Status, p.CategoryId,
                p.CreatedAt, p.UpdatedAt, p.CreatedBy, p.UpdatedBy,
                c.Id, c.Name, c.Description, c.CreatedAt, c.UpdatedAt, c.CreatedBy, c.UpdatedBy
            FROM Products p
            INNER JOIN Categories c ON p.CategoryId = c.Id
            WHERE (@Search IS NULL
                OR p.Name LIKE '%' + @Search + '%'
                OR p.SKU LIKE '%' + @Search + '%'
                OR c.Name LIKE '%' + @Search + '%')
            ORDER BY p.Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var products = await connection.QueryAsync<Product, Category, Product>(
            sql,
            (product, category) =>
            {
                product.Category = category;
                return product;
            },
            new
            {
                Search = string.IsNullOrWhiteSpace(search) ? null : search,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            },
            splitOn: "Id");

        return products;
    }

    public async Task<int> GetTotalCountAsync(string? search, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var sql = """
            SELECT COUNT(*)
            FROM Products p
            INNER JOIN Categories c ON p.CategoryId = c.Id
            WHERE (@Search IS NULL
                OR p.Name LIKE '%' + @Search + '%'
                OR p.SKU LIKE '%' + @Search + '%'
                OR c.Name LIKE '%' + @Search + '%')
            """;

        return await connection.ExecuteScalarAsync<int>(sql,
            new { Search = string.IsNullOrWhiteSpace(search) ? null : search });
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var sql = """
            SELECT
                p.Id, p.Name, p.SKU, p.Description, p.Price,
                p.StockQuantity, p.Status, p.CategoryId,
                p.CreatedAt, p.UpdatedAt, p.CreatedBy, p.UpdatedBy,
                c.Id, c.Name, c.Description, c.CreatedAt, c.UpdatedAt, c.CreatedBy, c.UpdatedBy
            FROM Products p
            INNER JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.Id = @Id
            """;

        var products = await connection.QueryAsync<Product, Category, Product>(
            sql,
            (product, category) =>
            {
                product.Category = category;
                return product;
            },
            new { Id = id },
            splitOn: "Id");

        return products.FirstOrDefault();
    }

    // ─── WRITE SIDE — EF Core ────────────────────────────────────────────

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        return product;
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Remove(product);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsBySkuAsync(string sku, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var sql = """
            SELECT COUNT(1)
            FROM Products
            WHERE SKU = @SKU
            AND (@ExcludeId IS NULL OR Id != @ExcludeId)
            """;

        var count = await connection.ExecuteScalarAsync<int>(sql, new { SKU = sku, ExcludeId = excludeId });
        return count > 0;
    }
}
