using Domain.Models;
using Repository.Generic;

namespace Repository.Contract;

public interface ITrackRepository : IGenericRepository<Track>
{
    Task<Track?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
