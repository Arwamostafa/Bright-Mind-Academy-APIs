using Domain.Common;
using Domain.Models;
using Repository.Contract;
using Repository.Generic;
using Service.Services.Contract;

namespace Service.Services.Implementation;

public class TrackService(ITrackRepository repo, IUnitOfWork unitOfWork) : ITrackService
{
    public Task<IReadOnlyList<Track>> GetAllTracksAsync(CancellationToken cancellationToken = default) =>
        repo.GetAllAsync(cancellationToken);

    public async Task<Result<Track>> GetTrackByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var track = await repo.GetByIdAsync(id, cancellationToken);
        return track is null
            ? Result.Failure<Track>(Error.NotFound("Track.NotFound", $"Track with id {id} was not found."))
            : Result.Success(track);
    }

    public async Task<Result<Track>> GetTrackByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var track = await repo.GetByNameAsync(name, cancellationToken);
        return track is null
            ? Result.Failure<Track>(Error.NotFound("Track.NotFound", $"Track named '{name}' was not found."))
            : Result.Success(track);
    }

    public async Task<Result<Track>> AddTrackAsync(Track addedTrack, CancellationToken cancellationToken = default)
    {
        await repo.AddAsync(addedTrack, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(addedTrack);
    }

    public async Task<Result> RemoveTrackByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var track = await repo.GetByIdAsync(id, cancellationToken);
        if (track is null)
            return Result.Failure(Error.NotFound("Track.NotFound", $"Track with id {id} was not found."));

        repo.Remove(track);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UpdateTrackByIdAsync(int id, Track updatedTrack, CancellationToken cancellationToken = default)
    {
        var track = await repo.GetByIdAsync(id, cancellationToken);
        if (track is null)
            return Result.Failure(Error.NotFound("Track.NotFound", $"Track with id {id} was not found."));

        track.TrackName = updatedTrack.TrackName;
        repo.Update(track);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
