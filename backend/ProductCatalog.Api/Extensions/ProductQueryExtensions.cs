using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Extensions;

public static class ProductQueryExtensions
{
    public static IQueryable<Product> InPriceRange(
        this IQueryable<Product> query, decimal? min, decimal? max)
    {
        if (min.HasValue) query = query.Where(p => p.Price >= min.Value);
        if (max.HasValue) query = query.Where(p => p.Price <= max.Value);
        return query;
    }

    public static IQueryable<Product> InStock(this IQueryable<Product> query)
        => query.Where(p => p.Quantity > 0);

    public static IQueryable<Product> OutOfStock(this IQueryable<Product> query)
        => query.Where(p => p.Quantity == 0);

    public static IQueryable<Product> LowStock(this IQueryable<Product> query, int threshold = 10)
        => query.Where(p => p.Quantity > 0 && p.Quantity <= threshold);

    public static IQueryable<Product> InCategory(this IQueryable<Product> query, int categoryId)
        => query.Where(p => p.CategoryId == categoryId);

    public static IQueryable<Product> SearchByText(this IQueryable<Product> query, string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return query;
        var lower = term.ToLower();
        return query.Where(p =>
            p.Name.ToLower().Contains(lower) ||
            p.Description.ToLower().Contains(lower));
    }

    public static IQueryable<Product> CreatedBetween(
        this IQueryable<Product> query, DateTime? from, DateTime? to)
    {
        if (from.HasValue) query = query.Where(p => p.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(p => p.CreatedAt <= to.Value);
        return query;
    }

    /// <summary>
    /// IEnumerable extension for in-memory collections.
    /// </summary>
    public static IEnumerable<Product> FuzzySearch(this IEnumerable<Product> products, string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return products;
        var lower = term.ToLower();
        return products.Where(p =>
            p.Name.ToLower().Contains(lower) ||
            p.Description.ToLower().Contains(lower) ||
            p.SKU.ToLower().Contains(lower));
    }
}