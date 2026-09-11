using Domain.Common;
using Domain.DTO;
using Domain.Models;

namespace Service.Services.Contract
{
    public interface ISubjectService
    {
        Task<List<SubjectWithUnits>> GetAllSubjectsAsync(CancellationToken cancellationToken = default);

        Task<Result<Subject>> GetSubjectByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Result<Subject>> GetSubjectByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<Result<CreatedSubjectDTO>> AddSubjectAsync(CreatedSubjectDTO addedSubject, CancellationToken cancellationToken = default);

        Task<Result> RemoveSubjectByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Result> UpdateSubjectByIdAsync(int id, CreatedSubjectDTO upSubjectDTO, CancellationToken cancellationToken = default);

        Task<List<SubjectDto>> TopThreeSubjectsAsync(CancellationToken cancellationToken = default);

        Task<List<SubjectDto>> GetPageOfSubjectsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<int> GetTotalSubjectsCountAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<StudentRegisterDTO>> GetStudentsBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default);

        Task<List<SubjectDto>> GetSubjectsByClassAndTrackAsync(int classId, int trackId, CancellationToken cancellationToken = default);

        Task<Result<SubjectDto>> GetHomeSubjectByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<List<SubjectDto>> GetHomeSubjectsAsync(CancellationToken cancellationToken = default);
    }
}
