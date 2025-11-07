using Domain.DTO;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repository;
using Repository.Contract;
using Repository.Implementation;
using Service.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class PaymentService : IPaymentService
        //: IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ISubjectRepository subjectRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly HttpClient _httpClient;

        public PaymentService(AppDbContext context, IConfiguration config , ISubjectRepository subjectRepository , IPaymentRepository paymentRepository)
        {
            _context = context;
            _config = config;
            this.subjectRepository = subjectRepository;
            _paymentRepository = paymentRepository;
            _httpClient = new HttpClient { BaseAddress = new Uri("https://accept.paymob.com/api/") };

        }
        
        
        #region Add Payment method
        public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto dto)
        {

            Console.WriteLine(dto == null ? "DTO is null" : "DTO is not null");
            // check if student exists
            var student = await _context.StudentProfiles.Include(s=>s.User).FirstOrDefaultAsync(s=>s.UserId==dto.StudentId);
            var subject = subjectRepository.GetById(dto.SubjectId);

            if (student == null || subject == null)
                throw new Exception("Student or Subject not found.");
            // check if student already enrolled in subject
            var existingRecord = await _context.SubjectStudents.FirstOrDefaultAsync(ss => ss.StudentId == dto.StudentId && ss.SubjectId == dto.SubjectId);

            if (existingRecord != null)
                throw new Exception("Student is already enrolled in this subject.");
            // add new record to SubjectStudent enrollment
            var subjectStudent = new SubjectStudent
            {
                StudentId = dto.StudentId,
                SubjectId = dto.SubjectId,
                IsPaid = false
            };
           await _paymentRepository.AddPayment(subjectStudent);
          await  _paymentRepository.SaveAsync();
            // make payment request to Paymob
            var authBody = new { api_key = _config["Paymob:ApiKey"] };
            var authResponse = await _httpClient.PostAsync( "auth/tokens", new StringContent(JsonConvert.SerializeObject(authBody), Encoding.UTF8, "application/json"));

            if (!authResponse.IsSuccessStatusCode)
                throw new Exception("Failed to authenticate with Paymob.");
            // get auth token
            var authJson = await authResponse.Content.ReadAsStringAsync();
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
            var orderResponse = await _httpClient.PostAsync("ecommerce/orders",new StringContent(JsonConvert.SerializeObject(orderRequest), Encoding.UTF8, "application/json"));
            // check order response
            if (!orderResponse.IsSuccessStatusCode)
            {
                var err = await orderResponse.Content.ReadAsStringAsync();
                throw new Exception($"Order registration failed: {err}");
            }
           
            var orderJson = await orderResponse.Content.ReadAsStringAsync();
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
            var paymentKeyResponse = await _httpClient.PostAsync("acceptance/payment_keys",new StringContent(JsonConvert.SerializeObject(paymentKeyRequest), Encoding.UTF8, "application/json"));
            // check payment key response
            if (!paymentKeyResponse.IsSuccessStatusCode)
            {
                var err = await paymentKeyResponse.Content.ReadAsStringAsync();
                throw new Exception($"Payment key request failed: {err}");
            }
            // get payment token
            var paymentKeyJson = await paymentKeyResponse.Content.ReadAsStringAsync();
            dynamic paymentKeyData = JsonConvert.DeserializeObject(paymentKeyJson);
            string paymentToken = paymentKeyData.token;

            // save  Transaction id to subjectStudent entrollment
            subjectStudent.TransactionId = orderId.ToString();
           await  _paymentRepository.UpdatePaymentAsync(subjectStudent);

            // get  iframe from paymob
            string iframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_config["Paymob:IframeId"]}?payment_token={paymentToken}";

            return new PaymentResponseDto
            {
                PaymentUrl = iframeUrl,
                TransactionId = orderId.ToString()
            };
        }
        #endregion


        #region  Callback function
        public async Task<string> HandlePaymentCallbackAsync(dynamic callbackData)
        {
            try
            {
                // Extract relevant data from callback
                string transactionId = callbackData?.obj?.id;
                bool isSuccess = callbackData?.obj?.success ?? false;
                string orderId = callbackData?.obj?.order?.id;

                if (string.IsNullOrEmpty(orderId))
                    return "Order ID is missing in callback data.";

                if (!isSuccess)
                    return "Payment failed or was cancelled.";

                var subjectStudent = await _context.SubjectStudents.FirstOrDefaultAsync(ss => ss.TransactionId == orderId);

                if (subjectStudent == null)
                    return $"No SubjectStudent found for order ID: {orderId}";
                // update payment status if payment is successful
                subjectStudent.IsPaid = true;
                subjectStudent.PaymentDate = DateTime.Now;
                subjectStudent.TransactionId = transactionId;

                await _context.SaveChangesAsync();

                return "Payment confirmed and data updated successfully.";
            }
            catch (Exception ex)
            {
                return $"Error while processing callback: {ex.Message}";
            }
        }

        #endregion


        #region calculate HmacSha256 to 
        public string computeHmacSha256(string message, string secret)
        {
            // Convert the secret and message to byte arrays
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            // Compute the HMACSHA512 hash
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hash = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        #endregion

        #region Update Payment Status
     public async Task<SubjectStudent> UpdatepaymentSuccess(string specialRefrence, decimal AmountPaid)
        {
            var subjectStudent = await _paymentRepository.GetPaymentsDetailsByTransactionId(specialRefrence);
            if (subjectStudent == null)
                throw new KeyNotFoundException("No SubjectStudent found for the provided reference.");

            subjectStudent.IsPaid = true;
            subjectStudent.PaymentDate = DateTime.Now;
            subjectStudent.Amount = AmountPaid;
            await _paymentRepository.UpdatePaymentAsync(subjectStudent);
            return subjectStudent;
        }
        public async Task<SubjectStudent> UpdatepaymentFaild(string specialRefrence, decimal AmountPaid)
        {
            var subjectStudent = await _context.SubjectStudents.FirstOrDefaultAsync(ss => ss.TransactionId == specialRefrence);
            if (subjectStudent == null)
                throw new KeyNotFoundException("No SubjectStudent found for the provided reference.");

            subjectStudent.IsPaid = false;
            subjectStudent.PaymentDate = DateTime.Now;
            subjectStudent.Amount = AmountPaid;
            await _paymentRepository.UpdatePaymentAsync(subjectStudent);
            return subjectStudent;
        }


        #endregion

        #region Get details of Payment
        public async Task<PaymentDTO> GetPaymentDetailsAsync(string transactionId)
        {
            var subjectStudent = await _paymentRepository.GetPaymentsDetailsByTransactionId(transactionId);
            if (subjectStudent == null)
                throw new KeyNotFoundException("No SubjectStudent found for the provided transaction ID.");
            return new PaymentDTO
            {
                TransactionId = subjectStudent.TransactionId,
                Amount = subjectStudent.Amount,
                StudentID = subjectStudent.StudentId,
                StudentName = subjectStudent.Student?.User?.FirstName + " " + subjectStudent.Student?.User?.LastName,
                SubjectID = subjectStudent.SubjectId,
                SubjectName = subjectStudent.Subject?.SubjectName,
                IsPaid = subjectStudent.IsPaid,
                PaymentDate = subjectStudent.PaymentDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A",
                InstructorId= subjectStudent.Subject?.InstructorID,
                InstructorName= subjectStudent.Subject?.Instructor?.User?.FirstName + " " + subjectStudent.Subject?.Instructor?.User?.LastName,

            };
        }

        #endregion

        #region Get all payment
        public async Task<List<PaymentDTO>> GetAllPayments()
        {
            var payments = await _paymentRepository.GetAllPayments();

            if (payments == null || !payments.Any())
                return new List<PaymentDTO>();

           
            return payments.Select(payment => new PaymentDTO
            {
              
                Amount = payment.Amount,
                StudentID = payment.StudentId,
                StudentName = payment.Student?.User?.FirstName + " " + payment.Student?.User?.LastName,
                SubjectID = payment.SubjectId,
                SubjectName = payment.Subject?.SubjectName,
                IsPaid = payment.IsPaid,
                PaymentDate = payment.PaymentDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A",
                TransactionId = payment.TransactionId,

            }).ToList();
        }

        #endregion

        #region get payment by student id and subject id 
        public async Task<PaymentDTO> GetPaymentsByStudentIdAndSubjectId(int studentId, int SubjectId)
        {
            var payment = await _context.SubjectStudents
                .Include(ps => ps.Student)
                .ThenInclude(s => s.User)
                .Include(ps => ps.Subject)
                .ThenInclude(sub=>sub.Instructor)
                .ThenInclude(i=>i.User)
                .Where(ps => ps.StudentId == studentId && ps.SubjectId == SubjectId)
                .FirstOrDefaultAsync();

            if (payment == null)
                return null;

            return new PaymentDTO
            {
                StudentID = payment.StudentId,
                StudentName = payment.Student?.User?.FirstName + " " + payment.Student?.User?.LastName,
                SubjectID = payment.SubjectId,
                SubjectName = payment.Subject?.SubjectName,
                IsPaid = payment.IsPaid,
                PaymentDate = payment.PaymentDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A",
                TransactionId = payment.TransactionId,
                InstructorId = payment.Subject?.InstructorID,
                InstructorName = payment.Subject?.Instructor?.User?.FirstName + " " + payment.Subject?.Instructor?.User?.LastName,
                Amount = payment.Amount


            };
        }

        #endregion

        #region number of stuednts entroll in subject
        public async Task<int> NumberOfStudentInSubject(int subjectId)
        {
            var count = await _paymentRepository.NumberOfStudentInSubject(subjectId);
            return count;
        }

        #endregion

       
    }
}



