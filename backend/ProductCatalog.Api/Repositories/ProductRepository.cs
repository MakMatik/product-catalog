using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<(IEnumerable<Product> Products, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, int? categoryId = null, string? search = null, string? sortBy = null);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ProductCatalogDbContext _context;

    public ProductRepository(ProductCatalogDbContext context)
    {
        _context = context;
    }

    public override async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(predicate)
            .ToListAsync();
    }

    public override async Task<Product> AddAsync(Product entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Products.Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(p => p.Category).LoadAsync();
        return entity;
    }

    public override async Task<Product> UpdateAsync(Product entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(p => p.Category).LoadAsync();
        return entity;
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return false;
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public override async Task<int> CountAsync(Expression<Func<Product, bool>>? predicate = null)
    {
        return predicate is null
            ? await _context.Products.CountAsync()
            : await _context.Products.CountAsync(predicate);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, int? categoryId = null, string? search = null, string? sortBy = null)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                p.Description.ToLower().Contains(searchLower) ||
                p.SKU.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "name" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "price" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderBy(p => p.Name)
        };

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }
}