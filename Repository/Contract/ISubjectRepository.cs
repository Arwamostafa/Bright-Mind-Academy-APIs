using Domain.Models;
using Repository.Generic;

namespace Repository.Contract
{
    public interface ISubjectRepository : IGenericRepository<Subject>
    {
        Task<IReadOnlyList<Subject>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

        Task<Subject?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<StudentProfile>> GetStudentsPaidBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<StudentClassSubject>> GetAllSubjectPaginationAsync(CancellationToken cancellationToken = default);

        Task<int> GetTotalSubjectsCountAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<StudentClassSubject>> GetByClassAndTrackAsync(int classId, int trackId, CancellationToken cancellationToken = default);

        Task<StudentClassSubject?> GetStudentClassSubjectBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default);
    }
}
