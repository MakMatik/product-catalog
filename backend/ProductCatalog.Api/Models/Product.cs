namespace ProductCatalog.Api.Models;

/// <summary>
/// Represents a product in the catalog. Implements IComparable for custom sorting.
/// </summary>
public class Product : IComparable<Product>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Category? Category { get; set; }

    /// <summary>
    /// Custom sort: by Category, then by Name, then by Price descending.
    /// </summary>
    public int CompareTo(Product? other)
    {
        if (other is null) return 1;

        // Primary sort: CategoryId ascending
        int categoryComparison = CategoryId.CompareTo(other.CategoryId);
        if (categoryComparison != 0) return categoryComparison;

        // Secondary sort: Name ascending
        int nameComparison = string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (nameComparison != 0) return nameComparison;

        // Tertiary sort: Price descending (reverse comparison)
        return other.Price.CompareTo(Price);
    }
}
