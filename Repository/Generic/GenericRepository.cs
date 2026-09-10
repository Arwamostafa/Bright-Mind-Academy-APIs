using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Repository.Generic;

public class GenericRepository<TEntity>(AppDbContext context) : IGenericRepository<TEntity>
    where TEntity : class
{
    private readonly DbSet<TEntity> _set = context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default) =>
        await _set.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking().AnyAsync(predicate, cancellationToken);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default) =>
        predicate is null
            ? await _set.AsNoTracking().CountAsync(cancellationToken)
            : await _set.AsNoTracking().CountAsync(predicate, cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await _set.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => _set.Update(entity);

    public void Remove(TEntity entity) => _set.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => _set.RemoveRange(entities);

    public IQueryable<TEntity> Query(bool asNoTracking = true) =>
        asNoTracking ? _set.AsNoTracking() : _set;
}
