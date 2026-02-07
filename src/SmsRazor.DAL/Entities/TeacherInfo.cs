using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

[Table("TeacherInfo")]
public class TeacherInfo
{
    [Key]
    public string TeacherCode { get; set; } = string.Empty;

    public Guid AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Account? Account { get; set; }

    public Guid DepartmentId { get; set; }
    [ForeignKey("DepartmentId")]
    public Department? Department { get; set; }

    public string? Title { get; set; }
}
