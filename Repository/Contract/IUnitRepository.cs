using Domain.Models;
using Repository.Generic;

namespace Repository.Contract;

public interface IUnitRepository : IGenericRepository<Unit>
{
    Task<IReadOnlyList<Unit>> GetAllWithSubjectAsync(CancellationToken cancellationToken = default);

    Task<Unit?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<Unit?> GetUnitByLessonIdAsync(int lessonId, CancellationToken cancellationToken = default);

    Task<List<Unit>> GetUnitsBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default);

    Task<List<Unit>> GetUnitsBySubjectNameAsync(string subjectName, CancellationToken cancellationToken = default);

    Task<int> GetUnitsCountBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default);
}
