using Domain.Common;
using Domain.DTO;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Application.Caching;
using Application.Repositories;
using Application.Services.Contract;

namespace Application.Services.Implementation
{
    public class SubjectQueryService(IUnitOfWork unitOfWork, ICacheService cache) : ISubjectQueryService
    {
        private const string AllSubjectsCacheKey = "subjects:all";
        private const string HomeSubjectsCacheKey = "subjects:home";
        private const string TopThreeSubjectsCacheKey = "subjects:top3";

        private IGenericRepository<StudentClassSubject> ClassSubjectRepo => unitOfWork.Repository<StudentClassSubject>();
        private IGenericRepository<SubjectStudent> SubjectStudentRepo => unitOfWork.Repository<SubjectStudent>();

        public Task<List<SubjectWithUnits>> GetAllSubjectsAsync(CancellationToken cancellationToken = default) =>
            cache.GetOrCreateAsync(AllSubjectsCacheKey, FetchAllSubjectsAsync, cancellationToken: cancellationToken);

        private async Task<List<SubjectWithUnits>> FetchAllSubjectsAsync(CancellationToken cancellationToken)
        {
            var subjects = await unitOfWork.Repository<Subject>().FindAllAsync(
                include: q => q.Include(s => s.Instructor).ThenInclude(i => i.User)
                               .Include(s => s.StudentClassSubject).ThenInclude(scs => scs.Class)
                               .Include(s => s.StudentClassSubject).ThenInclude(scs => scs.Track),
                cancellationToken: cancellationToken);

            return subjects.Select(subject =>
            {
                var scs = subject.StudentClassSubject;
                var user = subject.Instructor?.User;

                return new SubjectWithUnits
                {
                    SubjectID = subject.SubjectID,
                    SubjectName = subject.SubjectName,
                    SubjectDescription = subject.SubjectDescription,
                    InstructorID = subject.InstructorID,
                    InstructorName = user != null ? $"{user.FirstName} {user.LastName}" : string.Empty,
                    ClassName = scs?.Class?.ClassName ?? string.Empty,
                    TrackName = scs?.Track?.TrackName ?? string.Empty,
                    Price = subject.Price,
                    ClassID = scs?.ClassID ?? 0,
                    TrackID = scs?.TrackID ?? 0
                };
            }).ToList();
        }

        public Task<List<SubjectDto>> TopThreeSubjectsAsync(CancellationToken cancellationToken = default) =>
            cache.GetOrCreateAsync(TopThreeSubjectsCacheKey, FetchTopThreeSubjectsAsync, cancellationToken: cancellationToken);

        private async Task<List<SubjectDto>> FetchTopThreeSubjectsAsync(CancellationToken cancellationToken)
        {
            var topSubjectIds = await SubjectStudentRepo.Query()
                .Where(ss => ss.IsPaid)
                .GroupBy(ss => ss.SubjectId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToListAsync(cancellationToken);

            var subjects = await ClassSubjectRepo.FindAllAsync(
                predicate: s => topSubjectIds.Contains(s.SubjectID),
                include: q => q.Include(s => s.Instructor).ThenInclude(i => i.User)
                               .Include(s => s.Subject)
                               .Include(s => s.Class)
                               .Include(s => s.Track),
                cancellationToken: cancellationToken);

            var unitCounts = await GetUnitCountsAsync(subjects.Select(s => s.SubjectID), cancellationToken);

            return subjects.Select(s => new SubjectDto
            {
                SubjectId = s.SubjectID,
                SubjectName = s.Subject?.SubjectName,
                SubjectDescription = s.Subject?.SubjectDescription,
                SubjectPrice = s.Subject?.Price,
                ImgUrl = s.Instructor?.Image,
                ClassId = s.ClassID,
                ClassName = s.Class?.ClassName,
                InstructorName = s.Instructor?.User?.FirstName + " " + s.Instructor?.User?.LastName,
                TrackId = s.TrackID,
                TrackName = s.Track?.TrackName,
                unitCount = unitCounts.GetValueOrDefault(s.SubjectID)
            }).ToList();
        }

        public async Task<List<SubjectDto>> GetPageOfSubjectsAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var page = await ClassSubjectRepo.FindAllAsync(
                include: q => q.Include(s => s.Subject).ThenInclude(s => s.Instructor).ThenInclude(i => i.User)
                               .Include(s => s.Class)
                               .Include(s => s.Track),
                orderBy: q => q.OrderBy(cts => cts.SubjectID),
                skip: (pageNumber - 1) * pageSize,
                take: pageSize,
                cancellationToken: cancellationToken);

            var unitCounts = await GetUnitCountsAsync(page.Select(p => p.SubjectID), cancellationToken);

            return page.Select(cts => new SubjectDto
            {
                SubjectId = cts.SubjectID,
                InstructorName = cts.Subject?.Instructor?.User?.FirstName + " " + cts.Subject?.Instructor?.User?.LastName,
                SubjectName = cts.Subject?.SubjectName,
                SubjectPrice = cts.Subject?.Price,
                SubjectDescription = cts.Subject?.SubjectDescription,
                ImgUrl = cts.Subject?.Instructor?.Image,
                ClassId = cts.ClassID,
                ClassName = cts.Class?.ClassName,
                TrackId = cts.TrackID,
                TrackName = cts.Track?.TrackName,
                unitCount = unitCounts.GetValueOrDefault(cts.SubjectID)
            }).ToList();
        }

        public async Task<IEnumerable<StudentRegisterDTO>> GetStudentsBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default)
        {
            // Projects to the related Student rather than SubjectStudent itself - doesn't fit Find/FindAll.
            var students = await SubjectStudentRepo.Query()
                .Where(ss => ss.SubjectId == subjectId && ss.IsPaid)
                .Include(ss => ss.Student)
                    .ThenInclude(s => s.User)
                .Select(ss => ss.Student)
                .ToListAsync(cancellationToken);

            return students.Select(s => new StudentRegisterDTO
            {
                FirstName = s.User.FirstName,
                LastName = s.User.LastName,
                Email = s.User.Email ?? "dont have email ",
                PhoneNumber = s.User.PhoneNumber ?? "dont have phonenumber ",
                Age = s.Age,
                Address = s.User.Address ?? "dont have address ",
            }).ToList();
        }

        public async Task<List<SubjectDto>> GetSubjectsByClassAndTrackAsync(int classId, int trackId, CancellationToken cancellationToken = default)
        {
            var results = await ClassSubjectRepo.FindAllAsync(
                predicate: c => c.ClassID == classId && c.TrackID == trackId,
                include: q => q.Include(c => c.Subject).ThenInclude(s => s.Instructor).ThenInclude(i => i.User),
                cancellationToken: cancellationToken);

            return results.Select(cts => new SubjectDto
            {
                SubjectId = cts.SubjectID,
                InstructorName = cts.Subject?.Instructor?.User?.FirstName + " " + cts.Subject?.Instructor?.User?.LastName,
                SubjectName = cts.Subject?.SubjectName,
                SubjectPrice = cts.Subject?.Price,
                SubjectDescription = cts.Subject?.SubjectDescription,
                ImgUrl = cts.Subject?.Instructor?.Image
            }).ToList();
        }

        public async Task<Result<SubjectDto>> GetHomeSubjectByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var cts = await ClassSubjectRepo.FindAsync(
                c => c.SubjectID == id,
                include: q => q.Include(c => c.Subject).ThenInclude(s => s.Instructor).ThenInclude(i => i.User),
                cancellationToken: cancellationToken);

            if (cts is null)
                return Result.Failure<SubjectDto>(Error.NotFound("Subject.NotFound", $"Subject with id {id} was not found."));

            return Result.Success(new SubjectDto
            {
                SubjectId = cts.SubjectID,
                InstructorName = cts.Subject?.Instructor?.User?.FirstName + " " + cts.Subject?.Instructor?.User?.LastName,
                SubjectName = cts.Subject?.SubjectName,
                SubjectPrice = cts.Subject?.Price,
                SubjectDescription = cts.Subject?.SubjectDescription,
                ImgUrl = cts.Subject?.Instructor?.Image
            });
        }

        public Task<List<SubjectDto>> GetHomeSubjectsAsync(CancellationToken cancellationToken = default) =>
            cache.GetOrCreateAsync(HomeSubjectsCacheKey, FetchHomeSubjectsAsync, cancellationToken: cancellationToken);

        private async Task<List<SubjectDto>> FetchHomeSubjectsAsync(CancellationToken cancellationToken)
        {
            var subjects = await ClassSubjectRepo.FindAllAsync(
                include: q => q.Include(s => s.Subject).ThenInclude(s => s.Instructor).ThenInclude(i => i.User)
                               .Include(s => s.Class)
                               .Include(s => s.Track),
                cancellationToken: cancellationToken);

            var unitCounts = await GetUnitCountsAsync(subjects.Select(s => s.SubjectID), cancellationToken);

            return subjects.Select(cts => new SubjectDto
            {
                SubjectId = cts.SubjectID,
                InstructorName = cts.Subject?.Instructor?.User?.FirstName + " " + cts.Subject?.Instructor?.User?.LastName,
                SubjectName = cts.Subject?.SubjectName,
                SubjectPrice = cts.Subject?.Price,
                SubjectDescription = cts.Subject?.SubjectDescription,
                ImgUrl = cts.Subject?.Instructor?.Image,
                ClassId = cts.ClassID,
                ClassName = cts.Class?.ClassName,
                TrackId = cts.TrackID,
                TrackName = cts.Track?.TrackName,
                unitCount = unitCounts.GetValueOrDefault(cts.SubjectID)
            }).ToList();
        }

        public async Task InvalidateCachesAsync(CancellationToken cancellationToken = default)
        {
            await cache.RemoveAsync(AllSubjectsCacheKey, cancellationToken);
            await cache.RemoveAsync(HomeSubjectsCacheKey, cancellationToken);
            await cache.RemoveAsync(TopThreeSubjectsCacheKey, cancellationToken);
        }

        private async Task<Dictionary<int, int>> GetUnitCountsAsync(IEnumerable<int> subjectIds, CancellationToken cancellationToken)
        {
            var ids = subjectIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<int, int>();

            // Grouping/projection to a dictionary doesn't fit Find/FindAll - use the raw queryable escape hatch.
            return await unitOfWork.Repository<Unit>().Query()
                .Where(u => ids.Contains(u.SubjectId))
                .GroupBy(u => u.SubjectId)
                .Select(g => new { SubjectId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SubjectId, x => x.Count, cancellationToken);
        }
    }
}
