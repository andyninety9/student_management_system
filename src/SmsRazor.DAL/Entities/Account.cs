using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

[Table("Account")]
public class Account : BaseEntity
{
    [Key]
    public Guid AccountId { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public DateTime? Dob { get; set; }

    [MaxLength(100)]
    public string? Fullname { get; set; }

    [MaxLength(100)]
    public string? Gender { get; set; }

    public string? AvatarUrl { get; set; }

    public Guid RoleId { get; set; }
    [ForeignKey("RoleId")]
    public Role? Role { get; set; }

    public bool IsActive { get; set; } = true;

    public bool EmailVerified { get; set; } = false;
}
