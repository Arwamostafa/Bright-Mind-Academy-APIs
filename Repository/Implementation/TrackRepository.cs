using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Contract;
using Repository.Generic;

namespace Repository.Implementation;

public class TrackRepository(AppDbContext context) : GenericRepository<Track>(context), ITrackRepository
{
    public Task<Track?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        Query().SingleOrDefaultAsync(t => t.TrackName == name, cancellationToken);
}
