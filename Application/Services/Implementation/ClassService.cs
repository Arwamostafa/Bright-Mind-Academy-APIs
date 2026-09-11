using Domain.Common;
using Domain.Models;
using Application.Caching;
using Application.Repositories;
using Application.Services.Contract;

namespace Application.Services.Implementation;

public class ClassService(IUnitOfWork unitOfWork, ICacheService cache) : IClassService
{
    private const string AllClassesCacheKey = "classes:all";

    private IGenericRepository<Class> Repo => unitOfWork.Repository<Class>();

    public Task<IReadOnlyList<Class>> GetAllClassesAsync(CancellationToken cancellationToken = default) =>
        cache.GetOrCreateAsync(AllClassesCacheKey, Repo.GetAllAsync, cancellationToken: cancellationToken);

    public Task<PaginatedList<Class>> GetPageOfClassesAsync(RequestFilters requestFilters, CancellationToken cancellationToken = default) =>
        Repo.GetPaginatedListAsync(requestFilters, orderBy: q => q.OrderBy(c => c.ClassID), cancellationToken: cancellationToken);

    public async Task<Result<Class>> GetClassByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var @class = await Repo.GetByIdAsync(id, cancellationToken);
        return @class is null
            ? Result.Failure<Class>(Error.NotFound("Class.NotFound", $"Class with id {id} was not found."))
            : Result.Success(@class);
    }

    public async Task<Result<Class>> GetClassByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var @class = await Repo.FindAsync(c => c.ClassName == name, cancellationToken: cancellationToken);
        return @class is null
            ? Result.Failure<Class>(Error.NotFound("Class.NotFound", $"Class named '{name}' was not found."))
            : Result.Success(@class);
    }

    public async Task<Result<Class>> AddClassAsync(Class addedClass, CancellationToken cancellationToken = default)
    {
        await Repo.AddAsync(addedClass, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(AllClassesCacheKey, cancellationToken);
        return Result.Success(addedClass);
    }

    public async Task<Result> RemoveClassByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var @class = await Repo.GetByIdAsync(id, cancellationToken);
        if (@class is null)
            return Result.Failure(Error.NotFound("Class.NotFound", $"Class with id {id} was not found."));

        Repo.Remove(@class);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(AllClassesCacheKey, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UpdateClassByIdAsync(int id, Class updatedClass, CancellationToken cancellationToken = default)
    {
        var @class = await Repo.GetByIdAsync(id, cancellationToken);
        if (@class is null)
            return Result.Failure(Error.NotFound("Class.NotFound", $"Class with id {id} was not found."));

        @class.ClassName = updatedClass.ClassName;
        Repo.Update(@class);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(AllClassesCacheKey, cancellationToken);
        return Result.Success();
    }
}
