using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Application.Repositories;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Infrastructure.Repositories;

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

    public async Task<PaginatedList<TEntity>> GetPaginatedListAsync(
        RequestFilters requestFilters,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>>? include = null,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(include, asNoTracking, asSplitQuery: false);

        if (predicate is not null)
            query = query.Where(predicate);

        if (!string.IsNullOrWhiteSpace(requestFilters.SortColumn))
            query = query.OrderBy($"{requestFilters.SortColumn} {(requestFilters.IsAscending ? SortDirection.Ascending : SortDirection.Descending)}");
        else if (orderBy is not null)
            query = orderBy(query);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((requestFilters.PageNumber - 1) * requestFilters.PageSize)
            .Take(requestFilters.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<TEntity>(items, requestFilters.PageNumber, totalCount, requestFilters.PageSize);
    }

    public Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>>? include = null,
        bool asNoTracking = true,
        bool asSplitQuery = false,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(include, asNoTracking, asSplitQuery);
        return query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool asNoTracking = true,
        bool asSplitQuery = false,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(include, asNoTracking, asSplitQuery);

        if (predicate is not null)
            query = query.Where(predicate);

        if (orderBy is not null)
            query = orderBy(query);

        if (skip is not null)
            query = query.Skip(skip.Value);

        if (take is not null)
            query = query.Take(take.Value);

        return await query.ToListAsync(cancellationToken);
    }

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

    private IQueryable<TEntity> BuildQuery(
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>>? include,
        bool asNoTracking,
        bool asSplitQuery)
    {
        IQueryable<TEntity> query = Set;

        if (include is not null)
            query = include(query);

        if (asSplitQuery)
            query = query.AsSplitQuery();

        if (asNoTracking)
            query = query.AsNoTracking();

        return query;
    }
}
