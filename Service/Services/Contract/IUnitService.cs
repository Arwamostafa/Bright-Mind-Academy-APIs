using Domain.Common;
using Domain.DTO;
using Domain.Models;

namespace Service.Services.Contract
{
    public interface IUnitService
    {
        Task<IEnumerable<UnitDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<UnitWithSubjectAndLessonsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task AddAsync(UnitCreateDto unitDto, CancellationToken cancellationToken = default);

        Task<Result> Update(UnitCreateDto unitDto, int id, CancellationToken cancellationToken = default);

        Task<Result> Delete(int id, CancellationToken cancellationToken = default);
        Task<Result<UnitWithSubjectAndLessonsDto>> GetUnitByLessonId(int lessonId, CancellationToken cancellationToken = default);
        Task<List<Unit>> GetUnitsBySubjectId(int subjectId, CancellationToken cancellationToken = default);

        Task<List<Unit>> GetUnitsBySubjectName(string subjectname, CancellationToken cancellationToken = default);
        Task<int> GetNumberOfUnitsBySubjectId(int subjectId, CancellationToken cancellationToken = default);
    }
}
