using Domain.Common;
using Domain.DTO;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repository.Generic;
using Service.Services.Contract;
using System.Security.Cryptography;
using System.Text;

namespace Service.Services.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;

        private IGenericRepository<SubjectStudent> PaymentRepo => _unitOfWork.Repository<SubjectStudent>();

        public PaymentService(IConfiguration config, IUnitOfWork unitOfWork)
        {
            _config = config;
            _unitOfWork = unitOfWork;
            _httpClient = new HttpClient { BaseAddress = new Uri("https://accept.paymob.com/api/") };
        }


        #region Add Payment method
        public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default)
        {
            var student = await _unitOfWork.Repository<StudentProfile>()
                .FindAsync(s => s.UserId == dto.StudentId, include: q => q.Include(s => s.User), cancellationToken: cancellationToken);
            var subject = await _unitOfWork.Repository<Subject>().GetByIdAsync(dto.SubjectId, cancellationToken);

            if (student == null || subject == null)
                throw new Exception("Student or Subject not found.");
            // check if student already enrolled in subject
            var existingRecord = await PaymentRepo.FindAsync(
                ss => ss.StudentId == dto.StudentId && ss.SubjectId == dto.SubjectId, cancellationToken: cancellationToken);

            if (existingRecord != null)
                throw new Exception("Student is already enrolled in this subject.");
            // add new record to SubjectStudent enrollment
            var subjectStudent = new SubjectStudent
            {
                StudentId = dto.StudentId,
                SubjectId = dto.SubjectId,
                IsPaid = false
            };
            await PaymentRepo.AddAsync(subjectStudent, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            // make payment request to Paymob
            var authBody = new { api_key = _config["Paymob:ApiKey"] };
            var authResponse = await _httpClient.PostAsync("auth/tokens", new StringContent(JsonConvert.SerializeObject(authBody), Encoding.UTF8, "application/json"), cancellationToken);

            if (!authResponse.IsSuccessStatusCode)
                throw new Exception("Failed to authenticate with Paymob.");
            // get auth token
            var authJson = await authResponse.Content.ReadAsStringAsync(cancellationToken);
            dynamic authData = JsonConvert.DeserializeObject(authJson);
            string authToken = authData.token;
            // create order
            var orderRequest = new
            {
                auth_token = authToken,
                delivery_needed = "false",
                amount_cents = ((int)(subject.Price * 100)).ToString(),
                currency = "EGP",
                shipping_data = new
                {
                    apartment = "NA",
                    floor = "NA",
                    building = "NA",
                    street = student.User.Address ?? "N/A",
                    city = "Cairo",
                    country = "EG",
                    email = student.User.Email ?? "test@example.com",
                    first_name = student.User.FirstName ?? "N/A",
                    last_name = student.User.LastName ?? "N/A",
                    phone_number = student.User.PhoneNumber ?? "01000000000",
                    postal_code = "NA",
                    state = "NA"
                },
                items = new[]
                {
            new
            {
                name = subject.SubjectName ?? "Course",
                amount_cents = (int)(subject.Price * 100),
                quantity = 1
            }
        }
            };
            // create order request
            var orderResponse = await _httpClient.PostAsync("ecommerce/orders", new StringContent(JsonConvert.SerializeObject(orderRequest), Encoding.UTF8, "application/json"), cancellationToken);
            // check order response
            if (!orderResponse.IsSuccessStatusCode)
            {
                var err = await orderResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Order registration failed: {err}");
            }

            var orderJson = await orderResponse.Content.ReadAsStringAsync(cancellationToken);
            dynamic orderData = JsonConvert.DeserializeObject(orderJson);
            int orderId = orderData.id;
            var paymentKeyRequest = new
            {
                auth_token = authToken,
                amount_cents = (int)(subject.Price * 100),
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    apartment = "NA",
                    floor = "NA",
                    building = "NA",
                    street = student.User.Address ?? "N/A",
                    city = "Cairo",
                    country = "EG",
                    email = student.User.Email ?? "test@example.com",
                    first_name = student.User.FirstName ?? "N/A",
                    last_name = student.User.LastName ?? "N/A",
                    phone_number = student.User.PhoneNumber ?? "01000000000",
                    postal_code = "NA",
                    state = "NA"
                },
                currency = "EGP",
                integration_id = int.Parse(_config["Paymob:IntegrationId"])
            };
            // make payment key request
            var paymentKeyResponse = await _httpClient.PostAsync("acceptance/payment_keys", new StringContent(JsonConvert.SerializeObject(paymentKeyRequest), Encoding.UTF8, "application/json"), cancellationToken);
            // check payment key response
            if (!paymentKeyResponse.IsSuccessStatusCode)
            {
                var err = await paymentKeyResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Payment key request failed: {err}");
            }
            // get payment token
            var paymentKeyJson = await paymentKeyResponse.Content.ReadAsStringAsync(cancellationToken);
            dynamic paymentKeyData = JsonConvert.DeserializeObject(paymentKeyJson);
            string paymentToken = paymentKeyData.token;

            // save  Transaction id to subjectStudent entrollment
            subjectStudent.TransactionId = orderId.ToString();
            PaymentRepo.Update(subjectStudent);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // get  iframe from paymob
            string iframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_config["Paymob:IframeId"]}?payment_token={paymentToken}";

            return new PaymentResponseDto
            {
                PaymentUrl = iframeUrl,
                TransactionId = orderId.ToString()
            };
        }
        #endregion


        #region  calculate HmacSha256
        public string ComputeHmacSha256(string message, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        #endregion

        #region Update Payment Status
        public async Task<Result<SubjectStudent>> UpdatePaymentSuccessAsync(string specialReference, decimal amountPaid, CancellationToken cancellationToken = default)
        {
            var subjectStudent = await GetPaymentDetailsEntityAsync(specialReference, cancellationToken);
            if (subjectStudent == null)
                return Result.Failure<SubjectStudent>(Error.NotFound("Payment.NotFound", "No SubjectStudent found for the provided reference."));

            subjectStudent.IsPaid = true;
            subjectStudent.PaymentDate = DateTime.UtcNow;
            subjectStudent.Amount = amountPaid;
            PaymentRepo.Update(subjectStudent);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(subjectStudent);
        }

        public async Task<Result<SubjectStudent>> UpdatePaymentFailedAsync(string specialReference, decimal amountPaid, CancellationToken cancellationToken = default)
        {
            var subjectStudent = await GetPaymentDetailsEntityAsync(specialReference, cancellationToken);
            if (subjectStudent == null)
                return Result.Failure<SubjectStudent>(Error.NotFound("Payment.NotFound", "No SubjectStudent found for the provided reference."));

            subjectStudent.IsPaid = false;
            subjectStudent.PaymentDate = DateTime.UtcNow;
            subjectStudent.Amount = amountPaid;
            PaymentRepo.Update(subjectStudent);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(subjectStudent);
        }


        #endregion

        #region Get details of Payment
        public async Task<Result<PaymentDTO>> GetPaymentDetailsAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            var subjectStudent = await GetPaymentDetailsEntityAsync(transactionId, cancellationToken);
            if (subjectStudent == null)
                return Result.Failure<PaymentDTO>(Error.NotFound("Payment.NotFound", "No SubjectStudent found for the provided transaction ID."));

            return Result.Success(MapToDto(subjectStudent));
        }

        #endregion

        #region Get all payment
        public async Task<List<PaymentDTO>> GetAllPaymentsAsync(CancellationToken cancellationToken = default)
        {
            var payments = await PaymentRepo.FindAllAsync(
                include: q => q.Include(ps => ps.Student).ThenInclude(s => s.User)
                               .Include(ps => ps.Subject),
                cancellationToken: cancellationToken);

            return payments.Select(MapToDto).ToList();
        }

        #endregion

        #region get payment by student id and subject id
        public async Task<Result<PaymentDTO>> GetPaymentsByStudentIdAndSubjectIdAsync(int studentId, int subjectId, CancellationToken cancellationToken = default)
        {
            var payment = await PaymentRepo.FindAsync(
                ps => ps.StudentId == studentId && ps.SubjectId == subjectId,
                include: q => q.Include(ps => ps.Student).ThenInclude(s => s.User)
                               .Include(ps => ps.Subject).ThenInclude(sub => sub.Instructor).ThenInclude(i => i.User),
                cancellationToken: cancellationToken);

            if (payment == null)
                return Result.Failure<PaymentDTO>(Error.NotFound("Payment.NotFound", "Payment not found."));

            return Result.Success(MapToDto(payment));
        }

        #endregion

        #region number of stuednts entroll in subject
        public Task<int> NumberOfStudentInSubjectAsync(int subjectId, CancellationToken cancellationToken = default) =>
            PaymentRepo.CountAsync(ss => ss.SubjectId == subjectId && ss.IsPaid, cancellationToken);

        #endregion

        private Task<SubjectStudent?> GetPaymentDetailsEntityAsync(string transactionId, CancellationToken cancellationToken) =>
            PaymentRepo.FindAsync(
                ss => ss.TransactionId == transactionId,
                include: q => q.Include(ss => ss.Student).ThenInclude(s => s.User)
                               .Include(ss => ss.Subject).ThenInclude(sub => sub.Instructor).ThenInclude(i => i.User),
                cancellationToken: cancellationToken);

        private static PaymentDTO MapToDto(SubjectStudent payment) => new()
        {
            TransactionId = payment.TransactionId,
            Amount = payment.Amount,
            StudentID = payment.StudentId,
            StudentName = payment.Student?.User?.FirstName + " " + payment.Student?.User?.LastName,
            SubjectID = payment.SubjectId,
            SubjectName = payment.Subject?.SubjectName,
            IsPaid = payment.IsPaid,
            PaymentDate = payment.PaymentDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A",
            InstructorId = payment.Subject?.InstructorID,
            InstructorName = payment.Subject?.Instructor?.User?.FirstName + " " + payment.Subject?.Instructor?.User?.LastName,
        };
    }
}
