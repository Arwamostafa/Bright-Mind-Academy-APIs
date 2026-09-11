using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Contract;
using Repository.Generic;

namespace Repository.Implementation
{
    public class PaymentRepository(AppDbContext context) : GenericRepository<SubjectStudent>(context), IPaymentRepository
    {
        public async Task<IReadOnlyList<SubjectStudent>> GetAllPaymentsAsync(CancellationToken cancellationToken = default) =>
            await Query()
                .Include(ps => ps.Student)
                    .ThenInclude(s => s.User)
                .Include(ps => ps.Subject)
                .ToListAsync(cancellationToken);

        public async Task<SubjectStudent?> GetPaymentByStudentIdAndSubjectIdAsync(int studentId, int subjectId, CancellationToken cancellationToken = default) =>
            await Query()
                .Include(ps => ps.Student)
                    .ThenInclude(s => s.User)
                .Include(ps => ps.Subject)
                    .ThenInclude(sub => sub.Instructor)
                        .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(ps => ps.StudentId == studentId && ps.SubjectId == subjectId, cancellationToken);

        public async Task<SubjectStudent?> GetPaymentsDetailsByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default) =>
            await Query()
                .Include(ss => ss.Student)
                    .ThenInclude(s => s.User)
                .Include(ss => ss.Subject)
                    .ThenInclude(sub => sub.Instructor)
                        .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(ss => ss.TransactionId == transactionId, cancellationToken);

        public Task<int> NumberOfStudentInSubjectAsync(int subjectId, CancellationToken cancellationToken = default) =>
            Query().CountAsync(ss => ss.SubjectId == subjectId && ss.IsPaid, cancellationToken);

        public async Task<IReadOnlyList<StudentClassSubject>> TopThreeSubjectsAsync(CancellationToken cancellationToken = default)
        {
            var topSubjectIds = await Query()
                .Where(ss => ss.IsPaid)
                .GroupBy(ss => ss.SubjectId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToListAsync(cancellationToken);

            return await Context.StudentClassSubjects
                .AsNoTracking()
                .Where(s => topSubjectIds.Contains(s.SubjectID))
                .Include(s => s.Instructor)
                    .ThenInclude(i => i.User)
                .Include(s => s.Subject)
                .Include(s => s.Class)
                .Include(s => s.Track)
                .ToListAsync(cancellationToken);
        }
    }
}
