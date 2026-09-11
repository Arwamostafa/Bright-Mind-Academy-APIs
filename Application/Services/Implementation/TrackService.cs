using Domain.Common;
using Domain.Models;
using Application.Repositories;
using Application.Services.Contract;

namespace Application.Services.Implementation;

public class TrackService(IUnitOfWork unitOfWork) : ITrackService
{
    private IGenericRepository<Track> Repo => unitOfWork.Repository<Track>();

    public Task<IReadOnlyList<Track>> GetAllTracksAsync(CancellationToken cancellationToken = default) =>
        Repo.GetAllAsync(cancellationToken);

    public Task<PaginatedList<Track>> GetPageOfTracksAsync(RequestFilters requestFilters, CancellationToken cancellationToken = default) =>
        Repo.GetPaginatedListAsync(requestFilters, orderBy: q => q.OrderBy(t => t.TrackID), cancellationToken: cancellationToken);

    public async Task<Result<Track>> GetTrackByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var track = await Repo.GetByIdAsync(id, cancellationToken);
        return track is null
            ? Result.Failure<Track>(Error.NotFound("Track.NotFound", $"Track with id {id} was not found."))
            : Result.Success(track);
    }

    public async Task<Result<Track>> GetTrackByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var track = await Repo.FindAsync(t => t.TrackName == name, cancellationToken: cancellationToken);
        return track is null
            ? Result.Failure<Track>(Error.NotFound("Track.NotFound", $"Track named '{name}' was not found."))
            : Result.Success(track);
    }

    public async Task<Result<Track>> AddTrackAsync(Track addedTrack, CancellationToken cancellationToken = default)
    {
        await Repo.AddAsync(addedTrack, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(addedTrack);
    }

    public async Task<Result> RemoveTrackByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var track = await Repo.GetByIdAsync(id, cancellationToken);
        if (track is null)
            return Result.Failure(Error.NotFound("Track.NotFound", $"Track with id {id} was not found."));

        Repo.Remove(track);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UpdateTrackByIdAsync(int id, Track updatedTrack, CancellationToken cancellationToken = default)
    {
        var track = await Repo.GetByIdAsync(id, cancellationToken);
        if (track is null)
            return Result.Failure(Error.NotFound("Track.NotFound", $"Track with id {id} was not found."));

        track.TrackName = updatedTrack.TrackName;
        Repo.Update(track);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
