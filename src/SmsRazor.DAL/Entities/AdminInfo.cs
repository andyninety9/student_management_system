using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

[Table("AdminInfo")]
public class AdminInfo
{
    [Key]
    public string AdminID { get; set; } = string.Empty;

    public Guid AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Account? Account { get; set; }

    public Guid DepartmentId { get; set; }
    [ForeignKey("DepartmentId")]
    public Department? Department { get; set; }
}
