using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

public enum PaymentStatus
{
    Pending = 0,
    Success = 1,
    Failed = 2
}

[Table("TuitionPayments")]
public class TuitionPayment : BaseEntity
{
    [Key]
    public Guid TuitionPaymentId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(20)]
    public string StudentCode { get; set; } = string.Empty;

    public Guid TermId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(255)]
    public string? OrderInfo { get; set; }

    [MaxLength(255)]
    public string? VnPayTransactionId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(StudentCode))]
    public virtual StudentInfo? Student { get; set; }

    [ForeignKey(nameof(TermId))]
    public virtual Term? Term { get; set; }
}
