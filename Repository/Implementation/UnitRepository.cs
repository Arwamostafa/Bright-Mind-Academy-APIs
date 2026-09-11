using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Contract;
using Repository.Generic;

namespace Repository.Implementation
{
    public class UnitRepository(AppDbContext context) : GenericRepository<Unit>(context), IUnitRepository
    {
        public async Task<IReadOnlyList<Unit>> GetAllWithSubjectAsync(CancellationToken cancellationToken = default) =>
            await Query().Include(u => u.Subject).ToListAsync(cancellationToken);

        public async Task<Unit?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
            await Query()
                .Include(u => u.Subject)
                .Include(u => u.Lessons)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        public async Task<Unit?> GetUnitByLessonIdAsync(int lessonId, CancellationToken cancellationToken = default) =>
            await Context.Lessons
                .AsNoTracking()
                .Where(l => l.Id == lessonId)
                .Select(l => l.Unit)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<List<Unit>> GetUnitsBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default) =>
            await Context.Subjects
                .AsNoTracking()
                .Where(s => s.SubjectID == subjectId)
                .SelectMany(s => s.Units)
                .ToListAsync(cancellationToken);

        public async Task<List<Unit>> GetUnitsBySubjectNameAsync(string subjectName, CancellationToken cancellationToken = default) =>
            await Context.Subjects
                .AsNoTracking()
                .Where(s => s.SubjectName == subjectName)
                .SelectMany(s => s.Units)
                .ToListAsync(cancellationToken);

        public async Task<int> GetUnitsCountBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default) =>
            await Query().CountAsync(u => u.SubjectId == subjectId, cancellationToken);
    }
}
