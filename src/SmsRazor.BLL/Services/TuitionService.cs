using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmsRazor.BLL.Helpers;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

namespace SmsRazor.BLL.Services;

public class TuitionService : ITuitionService
{
    private readonly SmsDbContext _context;

    public TuitionService(SmsDbContext context)
    {
        _context = context;
    }

    public async Task<TuitionSummaryDTO> GetTuitionSummaryAsync(string studentCode, Guid termId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Section)
                .ThenInclude(s => s!.Course)
            .Where(e => e.StudentCode == studentCode && e.Section!.Calendars.Any(c => c.TermId == termId))
            .ToListAsync();

        var courses = enrollments.Select(e => new CourseTuitionItemDTO
        {
            CourseName = e.Section?.Course?.CourseNameEng ?? "Unknown",
            Credits = e.Section?.Course?.CreditNumber ?? 0,
            TuitionFee = e.Section?.Course?.TuitionFee ?? 0
        }).ToList();

        var totalAmount = courses.Sum(c => c.TuitionFee);

        var totalPaidAmount = await _context.TuitionPayments
            .Where(p => p.StudentCode == studentCode && p.TermId == termId && p.Status == PaymentStatus.Success)
            .SumAsync(p => p.Amount);

        var remainingBalance = totalAmount - totalPaidAmount;

        var payment = await _context.TuitionPayments
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(p => p.StudentCode == studentCode && p.TermId == termId);

        return new TuitionSummaryDTO
        {
            TermId = termId,
            StudentCode = studentCode,
            TotalAmount = totalAmount,
            TotalPaidAmount = totalPaidAmount,
            RemainingBalance = remainingBalance > 0 ? remainingBalance : 0,
            Courses = courses,
            PaymentStatus = payment?.Status ?? PaymentStatus.Pending,
            VnPayTransactionId = payment?.VnPayTransactionId,
            PaidAt = payment?.Status == PaymentStatus.Success ? payment.UpdatedAt : null
        };
    }

    public async Task<string> CreatePaymentUrlAsync(string studentCode, Guid termId, decimal amount, string ipAddress, IConfiguration config)
    {
        var tmnCode = config["VnPay:TmnCode"];
        var hashSecret = config["VnPay:HashSecret"];
        var baseUrl = config["VnPay:BaseUrl"];

        if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(baseUrl))
        {
            throw new Exception("VNPay configuration is missing in appsettings.");
        }

        // Save pending payment record
        var payment = new TuitionPayment
        {
            TuitionPaymentId = Guid.NewGuid(),
            StudentCode = studentCode,
            TermId = termId,
            Amount = amount,
            Status = PaymentStatus.Pending
        };
        _context.TuitionPayments.Add(payment);
        await _context.SaveChangesAsync();

        var vnpay = new VnPayLibrary();
        
        vnpay.AddRequestData("vnp_Version", config["VnPay:Version"] ?? "2.1.0");
        vnpay.AddRequestData("vnp_Command", config["VnPay:Command"] ?? "pay");
        vnpay.AddRequestData("vnp_TmnCode", tmnCode);
        vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString("0")); // Amount must be multiplied by 100
        
        // VNPay requires time in GMT+7 strictly
        var vnpayTime = DateTime.UtcNow.AddHours(7);
        vnpay.AddRequestData("vnp_CreateDate", vnpayTime.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_ExpireDate", vnpayTime.AddMinutes(15).ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", config["VnPay:CurrCode"] ?? "VND");
        vnpay.AddRequestData("vnp_IpAddr", ipAddress);
        vnpay.AddRequestData("vnp_Locale", config["VnPay:Locale"] ?? "vn");
        
        vnpay.AddRequestData("vnp_OrderInfo", $"Payment_for_term_{termId}");
        vnpay.AddRequestData("vnp_OrderType", "other"); // default
        
        var returnUrl = config["VnPay:ReturnUrl"];
        if (string.IsNullOrEmpty(returnUrl))
        {
            throw new Exception("VNPay ReturnUrl configuration is missing.");
        }
        
        vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
        vnpay.AddRequestData("vnp_TxnRef", payment.TuitionPaymentId.ToString()); // Using TuitionPaymentId as transaction ref

        var paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);

        return paymentUrl;
    }

    public async Task<bool> ProcessVnPayCallbackAsync(IQueryCollection query, IConfiguration config)
    {
        var hashSecret = config["VnPay:HashSecret"];
        var vnpay = new VnPayLibrary();

        foreach (var (key, value) in query)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value.ToString());
            }
        }

        var vnp_orderId = vnpay.GetResponseData("vnp_TxnRef");
        var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
        var vnp_SecureHash = query["vnp_SecureHash"].ToString();
        var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");

        bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, hashSecret!);
        
        if (checkSignature)
        {
            if (!Guid.TryParse(vnp_orderId, out Guid paymentId)) return false;

            var paymentRecord = await _context.TuitionPayments.FindAsync(paymentId);
            if (paymentRecord != null)
            {
                if (vnp_ResponseCode == "00")
                {
                    paymentRecord.Status = PaymentStatus.Success;
                    paymentRecord.VnPayTransactionId = vnp_TransactionId;
                }
                else
                {
                    paymentRecord.Status = PaymentStatus.Failed;
                    paymentRecord.VnPayTransactionId = vnp_TransactionId;
                }
                
                await _context.SaveChangesAsync();
                return paymentRecord.Status == PaymentStatus.Success;
            }
        }

        return false;
    }
}
