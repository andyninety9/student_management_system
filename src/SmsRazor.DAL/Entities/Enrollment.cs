using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

[Table("Enrollment")]
public class Enrollment : BaseEntity
{
    [Key]
    public Guid EnrollmentId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string StudentCode { get; set; } = string.Empty;

    [Required]
    public Guid SectionId { get; set; }

    [Required]
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(StudentCode))]
    public StudentInfo? Student { get; set; }

    [ForeignKey(nameof(SectionId))]
    public Section? Section { get; set; }
}
