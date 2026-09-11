using Domain.Common;
using Domain.Models;

namespace Service.Services.Contract;

public interface ITrackService
{
    Task<IReadOnlyList<Track>> GetAllTracksAsync(CancellationToken cancellationToken = default);

    Task<Result<Track>> GetTrackByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<Track>> GetTrackByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<Result<Track>> AddTrackAsync(Track addedTrack, CancellationToken cancellationToken = default);

    Task<Result> RemoveTrackByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result> UpdateTrackByIdAsync(int id, Track updatedTrack, CancellationToken cancellationToken = default);
}
