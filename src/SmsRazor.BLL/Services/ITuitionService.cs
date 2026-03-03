using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsRazor.DAL.Entities;

namespace SmsRazor.BLL.Services;

public class TuitionSummaryDTO
{
    public Guid TermId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? VnPayTransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public List<CourseTuitionItemDTO> Courses { get; set; } = new List<CourseTuitionItemDTO>();
}

public class CourseTuitionItemDTO
{
    public string CourseName { get; set; } = string.Empty;
    public int Credits { get; set; }
    public decimal TuitionFee { get; set; }
}

public interface ITuitionService
{
    Task<TuitionSummaryDTO> GetTuitionSummaryAsync(string studentCode, Guid termId);
    Task<string> CreatePaymentUrlAsync(string studentCode, Guid termId, decimal amount, string ipAddress, Microsoft.Extensions.Configuration.IConfiguration config);
    Task<bool> ProcessVnPayCallbackAsync(Microsoft.AspNetCore.Http.IQueryCollection query, Microsoft.Extensions.Configuration.IConfiguration config);
}
