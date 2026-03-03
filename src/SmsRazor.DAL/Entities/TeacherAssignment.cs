using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

[Table("TeacherAssignment")]
public class TeacherAssignment : BaseEntity
{
    [Key]
    public Guid TeacherAssignmentId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    [Column("teacherCode")]
    public string TeacherCode { get; set; } = string.Empty;

    // Navigation property
    [ForeignKey(nameof(TeacherCode))]
    public TeacherInfo? TeacherInfo { get; set; }

    // Optionally you can add Section references, but since it's 1 assignment to 1 section or many, 
    // depending on exact design. The design says Section has FK TeacherAssignmentId.
}
