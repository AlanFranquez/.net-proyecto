using System.Linq.Expressions;

namespace Espectaculos.Application.Abstractions.Repositories;

public interface IRepository<TEntity, TId> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
}