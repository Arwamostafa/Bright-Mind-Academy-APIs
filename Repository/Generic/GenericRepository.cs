using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Repository.Generic;

public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    protected AppDbContext Context { get; }
    protected DbSet<TEntity> Set { get; }

    public GenericRepository(AppDbContext context)
    {
        Context = context;
        Set = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default) =>
        await Set.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking().AnyAsync(predicate, cancellationToken);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default) =>
        predicate is null
            ? await Set.AsNoTracking().CountAsync(cancellationToken)
            : await Set.AsNoTracking().CountAsync(predicate, cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await Set.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => Set.Update(entity);

    public void Remove(TEntity entity) => Set.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => Set.RemoveRange(entities);

    public IQueryable<TEntity> Query(bool asNoTracking = true) =>
        asNoTracking ? Set.AsNoTracking() : Set;
}
