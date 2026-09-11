using Domain.Models;
using Repository.Generic;

namespace Repository.Contract;

public interface IClassRepository : IGenericRepository<Class>
{
    Task<Class?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
