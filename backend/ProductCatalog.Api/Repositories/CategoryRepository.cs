using System.Linq.Expressions;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetRootCategoriesAsync();
    Task<IEnumerable<Category>> GetChildrenAsync(int parentId);
}

/// <summary>
/// Uses pure in-memory collections (Dictionary, List) — NO Entity Framework.
/// </summary>
public class InMemoryCategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly Dictionary<int, Category> _categories = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    public InMemoryCategoryRepository()
    {
        SeedData();
    }

    private void SeedData()
    {
        var seed = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", Description = "Electronic devices and gadgets" },
            new() { Id = 2, Name = "Laptops", Description = "Portable computers", ParentCategoryId = 1 },
            new() { Id = 3, Name = "Smartphones", Description = "Mobile phones", ParentCategoryId = 1 },
            new() { Id = 4, Name = "Clothing", Description = "Apparel and accessories" },
            new() { Id = 5, Name = "Men's Wear", Description = "Clothing for men", ParentCategoryId = 4 },
            new() { Id = 6, Name = "Women's Wear", Description = "Clothing for women", ParentCategoryId = 4 },
            new() { Id = 7, Name = "Books", Description = "Physical and digital books" },
            new() { Id = 8, Name = "Fiction", Description = "Fiction books", ParentCategoryId = 7 },
            new() { Id = 9, Name = "Non-Fiction", Description = "Non-fiction books", ParentCategoryId = 7 }
        };

        foreach (var cat in seed)
            _categories[cat.Id] = cat;

        _nextId = _categories.Keys.Max() + 1;
    }

    public override Task<Category?> GetByIdAsync(int id)
    {
        lock (_lock)
        {
            _categories.TryGetValue(id, out var category);
            return Task.FromResult(category);
        }
    }

    public override Task<IEnumerable<Category>> GetAllAsync()
    {
        lock (_lock)
        {
            var all = _categories.Values.OrderBy(c => c.Name).ToList();
            return Task.FromResult<IEnumerable<Category>>(all);
        }
    }

    public override Task<IEnumerable<Category>> FindAsync(Expression<Func<Category, bool>> predicate)
    {
        lock (_lock)
        {
            var compiled = predicate.Compile();
            var results = _categories.Values.Where(compiled).ToList();
            return Task.FromResult<IEnumerable<Category>>(results);
        }
    }

    public override Task<Category> AddAsync(Category entity)
    {
        lock (_lock)
        {
            entity.Id = _nextId++;
            _categories[entity.Id] = entity;
            return Task.FromResult(entity);
        }
    }

    public override Task<Category> UpdateAsync(Category entity)
    {
        lock (_lock)
        {
            if (!_categories.ContainsKey(entity.Id))
                throw new KeyNotFoundException($"Category with Id {entity.Id} not found.");
            _categories[entity.Id] = entity;
            return Task.FromResult(entity);
        }
    }

    public override Task<bool> DeleteAsync(int id)
    {
        lock (_lock)
        {
            var hasChildren = _categories.Values.Any(c => c.ParentCategoryId == id);
            if (hasChildren) return Task.FromResult(false);
            return Task.FromResult(_categories.Remove(id));
        }
    }

    public override Task<int> CountAsync(Expression<Func<Category, bool>>? predicate = null)
    {
        lock (_lock)
        {
            if (predicate is null)
                return Task.FromResult(_categories.Count);
            var compiled = predicate.Compile();
            return Task.FromResult(_categories.Values.Count(compiled));
        }
    }

    public Task<IEnumerable<Category>> GetRootCategoriesAsync()
    {
        lock (_lock)
        {
            var roots = _categories.Values
                .Where(c => c.ParentCategoryId is null)
                .OrderBy(c => c.Name).ToList();
            return Task.FromResult<IEnumerable<Category>>(roots);
        }
    }

    public Task<IEnumerable<Category>> GetChildrenAsync(int parentId)
    {
        lock (_lock)
        {
            var children = _categories.Values
                .Where(c => c.ParentCategoryId == parentId)
                .OrderBy(c => c.Name).ToList();
            return Task.FromResult<IEnumerable<Category>>(children);
        }
    }
}