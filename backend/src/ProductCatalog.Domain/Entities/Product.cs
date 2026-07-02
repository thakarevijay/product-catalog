namespace ProductCatalog.Domain.Entities;

using ProductCatalog.Domain.Common;
using ProductCatalog.Domain.Enums;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
