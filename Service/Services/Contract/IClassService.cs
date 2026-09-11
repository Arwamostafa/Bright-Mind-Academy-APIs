using Domain.Common;
using Domain.Models;

namespace Service.Services.Contract;

public interface IClassService
{
    Task<IReadOnlyList<Class>> GetAllClassesAsync(CancellationToken cancellationToken = default);

    Task<Result<Class>> GetClassByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<Class>> GetClassByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<Result<Class>> AddClassAsync(Class addedClass, CancellationToken cancellationToken = default);

    Task<Result> RemoveClassByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result> UpdateClassByIdAsync(int id, Class updatedClass, CancellationToken cancellationToken = default);
}
