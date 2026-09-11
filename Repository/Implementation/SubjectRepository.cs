using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Contract;
using Repository.Generic;

namespace Repository.Implementation
{
    public class SubjectRepository(AppDbContext context) : GenericRepository<Subject>(context), ISubjectRepository
    {
        public async Task<IReadOnlyList<Subject>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default) =>
            await Query()
                .Include(s => s.Instructor)
                    .ThenInclude(i => i.User)
                .Include(s => s.StudentClassSubject)
                    .ThenInclude(scs => scs.Class)
                .Include(s => s.StudentClassSubject)
                    .ThenInclude(scs => scs.Track)
                .ToListAsync(cancellationToken);

        public Task<Subject?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
            Query().SingleOrDefaultAsync(c => c.SubjectName == name, cancellationToken);

        public async Task<IReadOnlyList<StudentProfile>> GetStudentsPaidBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default) =>
            await Context.SubjectStudents
                .AsNoTracking()
                .Where(ss => ss.SubjectId == subjectId && ss.IsPaid)
                .Include(ss => ss.Student)
                    .ThenInclude(s => s.User)
                .Select(ss => ss.Student)
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<StudentClassSubject>> GetAllSubjectPaginationAsync(CancellationToken cancellationToken = default) =>
            await Context.StudentClassSubjects
                .AsNoTracking()
                .Include(s => s.Subject)
                    .ThenInclude(s => s.Instructor)
                        .ThenInclude(i => i.User)
                .Include(s => s.Class)
                .Include(s => s.Track)
                .ToListAsync(cancellationToken);

        public Task<int> GetTotalSubjectsCountAsync(CancellationToken cancellationToken = default) =>
            Query().CountAsync(cancellationToken);

        public async Task<IReadOnlyList<StudentClassSubject>> GetByClassAndTrackAsync(int classId, int trackId, CancellationToken cancellationToken = default) =>
            await Context.StudentClassSubjects
                .AsNoTracking()
                .Where(c => c.ClassID == classId && c.TrackID == trackId)
                .Include(c => c.Subject)
                    .ThenInclude(s => s.Instructor)
                        .ThenInclude(i => i.User)
                .ToListAsync(cancellationToken);

        public Task<StudentClassSubject?> GetStudentClassSubjectBySubjectIdAsync(int subjectId, CancellationToken cancellationToken = default) =>
            Context.StudentClassSubjects.FirstOrDefaultAsync(sc => sc.SubjectID == subjectId, cancellationToken);
    }
}
