namespace Application.Repositories;

public interface IUnitOfWork
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
