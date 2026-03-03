using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

[Table("Role")]
public class Role : BaseEntity
{
    [Key]
    public Guid RoleId { get; set; }

    [Required]
    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
