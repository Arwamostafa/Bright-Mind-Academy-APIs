using Domain.Common;
using Domain.DTO;

namespace Application.Services.Contract
{
    public interface ILessonService
    {
        Task<IEnumerable<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<LessonDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Result> AddAsync(LessonCreateDto lessonDto, CancellationToken cancellationToken = default);

        Task<Result> Update(LessonCreateDto lessonDto, int id, CancellationToken cancellationToken = default);

        Task<IEnumerable<LessonDto>> GetLessonsByUnitId(int unitId, CancellationToken cancellationToken = default);

        Task<IEnumerable<LessonDto>> GetLessonsByUnitName(string unitname, CancellationToken cancellationToken = default);

        Task<Result> Delete(int id, CancellationToken cancellationToken = default);
    }
}
