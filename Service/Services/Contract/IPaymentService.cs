using Domain.Common;
using Domain.DTO;
using Domain.Models;

namespace Service.Services.Contract
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default);
        string ComputeHmacSha256(string message, string secret);
        Task<Result<SubjectStudent>> UpdatePaymentSuccessAsync(string specialReference, decimal amountPaid, CancellationToken cancellationToken = default);
        Task<Result<SubjectStudent>> UpdatePaymentFailedAsync(string specialReference, decimal amountPaid, CancellationToken cancellationToken = default);
        Task<Result<PaymentDTO>> GetPaymentDetailsAsync(string transactionId, CancellationToken cancellationToken = default);

        Task<List<PaymentDTO>> GetAllPaymentsAsync(CancellationToken cancellationToken = default);

        Task<Result<PaymentDTO>> GetPaymentsByStudentIdAndSubjectIdAsync(int studentId, int subjectId, CancellationToken cancellationToken = default);

        Task<int> NumberOfStudentInSubjectAsync(int subjectId, CancellationToken cancellationToken = default);
    }
}
