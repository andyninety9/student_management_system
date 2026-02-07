using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

[Table("StudentInfo")]
public class StudentInfo : BaseEntity
{
    [Key]
    public string StudentCode { get; set; } = string.Empty;

    public Guid AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Account? Account { get; set; }

    public Guid SyllabusId { get; set; }
    [ForeignKey("SyllabusId")]
    public Syllabus? Syllabus { get; set; }

    public Guid DepartmentId { get; set; }
    [ForeignKey("DepartmentId")]
    public Department? Department { get; set; }

    public Guid IntakeId { get; set; }
    [ForeignKey("IntakeId")]
    public Intake? Intake { get; set; }

    public Guid StudentStatusId { get; set; }
    [ForeignKey("StudentStatusId")]
    public StudentStatus? StudentStatus { get; set; }
}
