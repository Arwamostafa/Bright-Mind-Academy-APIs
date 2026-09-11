using Domain.Models;
using Repository.Generic;

namespace Repository.Contract
{
    public interface ILessonRepository : IGenericRepository<Lesson>
    {
        Task<IReadOnlyList<Lesson>> GetAllWithUnitAsync(CancellationToken cancellationToken = default);

        Task<Lesson?> GetWithUnitAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Lesson>> GetLessonsByUnitIdAsync(int unitId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Lesson>> GetLessonsByUnitNameAsync(string unitName, CancellationToken cancellationToken = default);
    }
}
