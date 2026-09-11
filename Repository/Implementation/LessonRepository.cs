using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Contract;
using Repository.Generic;

namespace Repository.Implementation
{
    public class LessonRepository(AppDbContext context) : GenericRepository<Lesson>(context), ILessonRepository
    {
        public async Task<IReadOnlyList<Lesson>> GetAllWithUnitAsync(CancellationToken cancellationToken = default) =>
            await Query().Include(l => l.Unit).ToListAsync(cancellationToken);

        public async Task<Lesson?> GetWithUnitAsync(int id, CancellationToken cancellationToken = default) =>
            await Query().Include(l => l.Unit).FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        public async Task<IReadOnlyList<Lesson>> GetLessonsByUnitIdAsync(int unitId, CancellationToken cancellationToken = default) =>
            await Query().Where(l => l.UnitId == unitId).ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<Lesson>> GetLessonsByUnitNameAsync(string unitName, CancellationToken cancellationToken = default) =>
            await Query().Where(l => l.Unit.Title == unitName).ToListAsync(cancellationToken);
    }
}
