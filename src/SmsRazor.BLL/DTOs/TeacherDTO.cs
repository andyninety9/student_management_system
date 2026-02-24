using System;
using System.ComponentModel.DataAnnotations;

namespace SmsRazor.BLL.DTOs;

public class TeacherDTO
{
    // Account Information
    public Guid AccountId { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "Full Name")]
    public string Fullname { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime? Dob { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    [Display(Name = "Account Status")]
    public bool IsActive { get; set; } = true;

    // Teacher Information
    [Required]
    [MaxLength(50)]
    [Display(Name = "Teacher Code")]
    public string TeacherCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Department")]
    public Guid DepartmentId { get; set; }

    [Display(Name = "Title")]
    public string? Title { get; set; }

    // Read-only specific fields for display
    public string? DepartmentName { get; set; }

    // Used only during Creation or Password Reset
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string? Password { get; set; }
}
