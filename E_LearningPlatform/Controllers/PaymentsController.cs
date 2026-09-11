using Domain.DTO;
using E_LearningPlatform.Extensions;
using E_LearningPlatform.Helper;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contract;
using System.Text;

namespace E_LearningPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _config;

        public PaymentsController(IPaymentService paymentService, IConfiguration config)
        {
            _paymentService = paymentService;
            _config = config;
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _paymentService.CreatePaymentAsync(dto, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("callback")]
        public async Task<IActionResult> PaymentCallback(CancellationToken cancellationToken)
        {
            // Extract query parameters
            PaymentPageInfoDto paymentPageInfoDto = new PaymentPageInfoDto();

            var query = Request.Query;
            string[] fields = new[]
              {
            "amount_cents","created_at","currency","error_occured","has_parent_transaction","id","integration_id","is_3d_secure","is_auth","is_capture","is_refunded","is_standalone_payment","is_voided","order","owner","pending","source_data.pan","source_data.sub_type","source_data.type","success"
              };
            // Concatenate field values
            var stringConcate = new StringBuilder();
            foreach (var field in fields)
            {

                if (query.TryGetValue(field, out var value))
                {
                    stringConcate.Append(value);
                }
                else
                {
                    return BadRequest($"Missing :{field}");
                }

            }
            // Compute HMAC SHA256
            string HmacRecived = query["hmac"];
            string calsulatedHmac = _paymentService.ComputeHmacSha256(stringConcate.ToString(), _config["Paymob:HmacSecret"]);
            // Compare HMACs
            if (HmacRecived.Equals(calsulatedHmac, StringComparison.OrdinalIgnoreCase))
            {    // HMAC is valid, process the payment
                bool.TryParse(query["success"], out bool Success);
                var TransactionId = query["order"];
                var errorCode = query["data.message"].ToString();
                var paymentDate = query["created_at"];

                var paymentMethod = query["source_data.type"];
                string reason = errorCode switch
                {
                    "ERR_PAYMENT_DECLINED" => "Insufficient Funds",
                    "SEC_AUTH_FAILED" => "3D Secure Verification Failed",
                    _ => "Unknown Error"
                };
                // Convert amount from cents to EGP
                decimal amountInEgp = 0;
                if (decimal.TryParse(query["amount_cents"], out decimal amountCents))
                {
                    amountInEgp = amountCents / 100;
                }
                paymentPageInfoDto = new PaymentPageInfoDto
                {
                    id = TransactionId,
                    AmountCents = amountInEgp,
                    DateTime = paymentDate,
                    ErrorCode = errorCode,
                    reason = reason,
                    paymentMethod = paymentMethod,

                };
                // Update payment status based on success and navigate to success page
                if (Success)
                {
                    await _paymentService.UpdatePaymentSuccessAsync(TransactionId!, amountInEgp, cancellationToken);
                    return Content(HtmlGenerator.GenerateSuccessHtml(paymentPageInfoDto), "text/html");
                }
                // Payment failed and navigate to failed page
                await _paymentService.UpdatePaymentFailedAsync(TransactionId!, amountInEgp, cancellationToken);
                return Content(HtmlGenerator.GenerateFailedHtml(paymentPageInfoDto), "text/html");
            }
            // HMAC is invalid, possible tampering detected and navigate to security page
            return Content(HtmlGenerator.GenerateSecurityHtml(paymentPageInfoDto), "text/html");

        }


        [HttpGet("DownloadReceipt")]
        public async Task<IActionResult> DownloadReceipt(string receiptId, CancellationToken cancellationToken)
        {
            var result = await _paymentService.GetPaymentDetailsAsync(receiptId, cancellationToken);
            if (result.IsFailure)
                return NotFound("Receipt not found.");

            var payment = result.Value;

            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                document.Add(new Paragraph("E-Learning Platform Receipt").SetTextAlignment(TextAlignment.CENTER).SimulateBold().SetFontSize(20));
                document.Add(new Paragraph($"Receipt ID: {receiptId}"));
                document.Add(new Paragraph($"Student Name: {payment.StudentName}"));
                document.Add(new Paragraph($"Amount Paid: {payment.Amount:EGP}"));
                document.Add(new Paragraph($"Date: {payment.PaymentDate}"));
                document.Add(new Paragraph("Thank you for your payment!").SetTextAlignment(TextAlignment.CENTER).SimulateBold().SetFontSize(12));

                document.Close();

                var fileBytes = stream.ToArray();
                return File(fileBytes, "application/pdf", $"Receipt_{receiptId}.pdf");
            }
        }


        [HttpGet("GetAllPayments")]
        public async Task<IActionResult> GetAllPayments(CancellationToken cancellationToken)
        {
            var payments = await _paymentService.GetAllPaymentsAsync(cancellationToken);
            return Ok(payments);
        }

        [HttpGet("GetPaymentDetalisBYTransactionId")]
        public async Task<IActionResult> GetPaymentDetalis(string transactionId, CancellationToken cancellationToken)
        {
            var result = await _paymentService.GetPaymentDetailsAsync(transactionId, cancellationToken);
            return result.ToActionResult(this);
        }



        [HttpGet("GetPaymentDetalisBYStudentIdAndSubjectId")]
        public async Task<IActionResult> GetPaymentsByStudentIdAndSubjectId(int studentId, int subjectId, CancellationToken cancellationToken)
        {
            var result = await _paymentService.GetPaymentsByStudentIdAndSubjectIdAsync(studentId, subjectId, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("NumberOfStudentsEnrolledInSubject")]
        public async Task<IActionResult> NumberOfStudentsEnrolledInSubject(int subjectId, CancellationToken cancellationToken)
        {
            var count = await _paymentService.NumberOfStudentInSubjectAsync(subjectId, cancellationToken);
            return Ok(count);
        }

    }

}
