using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Contract;
using Repository.Generic;

namespace Repository.Implementation;

public class ClassRepository(AppDbContext context) : GenericRepository<Class>(context), IClassRepository
{
    public Task<Class?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        Query().SingleOrDefaultAsync(c => c.ClassName == name, cancellationToken);
}
