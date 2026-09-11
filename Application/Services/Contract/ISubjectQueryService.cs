using Domain.Common;
using Domain.DTO;
using Domain.Models;

namespace Application.Services.Contract
{
    public interface ISubjectQueryService
    {
        Task<List<SubjectWithUnits>> GetAllSubjectsAsync(CancellationToken cancellationToken = default);

        Task<List<SubjectDto>> TopThreeSubjectsAsync(CancellationToken cancellationToken = default);

        Task<List<SubjectDto>> GetPageOfSubjectsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<IEnumerable<StudentRegisterDTO>> GetStudentsBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default);

        Task<List<SubjectDto>> GetSubjectsByClassAndTrackAsync(int classId, int trackId, CancellationToken cancellationToken = default);

        Task<Result<SubjectDto>> GetHomeSubjectByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<List<SubjectDto>> GetHomeSubjectsAsync(CancellationToken cancellationToken = default);

        Task InvalidateCachesAsync(CancellationToken cancellationToken = default);
    }
}
