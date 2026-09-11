using Domain.Models;
using Repository.Generic;

namespace Repository.Contract
{
    public interface IPaymentRepository : IGenericRepository<SubjectStudent>
    {
        Task<SubjectStudent?> GetPaymentsDetailsByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<SubjectStudent>> GetAllPaymentsAsync(CancellationToken cancellationToken = default);

        Task<SubjectStudent?> GetPaymentByStudentIdAndSubjectIdAsync(int studentId, int subjectId, CancellationToken cancellationToken = default);

        Task<int> NumberOfStudentInSubjectAsync(int subjectId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<StudentClassSubject>> TopThreeSubjectsAsync(CancellationToken cancellationToken = default);
    }
}
