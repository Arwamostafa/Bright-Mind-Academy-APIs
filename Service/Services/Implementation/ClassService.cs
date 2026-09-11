using Domain.Common;
using Domain.Models;
using Repository.Contract;
using Repository.Generic;
using Service.Services.Contract;

namespace Service.Services.Implementation;

public class ClassService(IClassRepository repo, IUnitOfWork unitOfWork) : IClassService
{
    public Task<IReadOnlyList<Class>> GetAllClassesAsync(CancellationToken cancellationToken = default) =>
        repo.GetAllAsync(cancellationToken);

    public async Task<Result<Class>> GetClassByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var @class = await repo.GetByIdAsync(id, cancellationToken);
        return @class is null
            ? Result.Failure<Class>(Error.NotFound("Class.NotFound", $"Class with id {id} was not found."))
            : Result.Success(@class);
    }

    public async Task<Result<Class>> GetClassByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var @class = await repo.GetByNameAsync(name, cancellationToken);
        return @class is null
            ? Result.Failure<Class>(Error.NotFound("Class.NotFound", $"Class named '{name}' was not found."))
            : Result.Success(@class);
    }

    public async Task<Result<Class>> AddClassAsync(Class addedClass, CancellationToken cancellationToken = default)
    {
        await repo.AddAsync(addedClass, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(addedClass);
    }

    public async Task<Result> RemoveClassByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var @class = await repo.GetByIdAsync(id, cancellationToken);
        if (@class is null)
            return Result.Failure(Error.NotFound("Class.NotFound", $"Class with id {id} was not found."));

        repo.Remove(@class);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UpdateClassByIdAsync(int id, Class updatedClass, CancellationToken cancellationToken = default)
    {
        var @class = await repo.GetByIdAsync(id, cancellationToken);
        if (@class is null)
            return Result.Failure(Error.NotFound("Class.NotFound", $"Class with id {id} was not found."));

        @class.ClassName = updatedClass.ClassName;
        repo.Update(@class);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
