using System.Linq.Expressions;

namespace ProductCatalog.Api.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}

public abstract class Repository<T> : IRepository<T> where T : class
{
    public abstract Task<T?> GetByIdAsync(int id);
    public abstract Task<IEnumerable<T>> GetAllAsync();
    public abstract Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    public abstract Task<T> AddAsync(T entity);
    public abstract Task<T> UpdateAsync(T entity);
    public abstract Task<bool> DeleteAsync(int id);
    public abstract Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}