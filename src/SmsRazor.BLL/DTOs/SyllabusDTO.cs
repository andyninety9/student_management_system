using System;
using System.ComponentModel.DataAnnotations;

namespace SmsRazor.BLL.DTOs;

public class SyllabusDTO
{
    public Guid SyllabusId { get; set; }

    [Required(ErrorMessage = "Please select a department.")]
    [Display(Name = "Department")]
    public Guid DepartmentId { get; set; }

    public string? DepartmentName { get; set; } // Read-only for display

    [Required]
    [MaxLength(255)]
    [Display(Name = "Syllabus Name")]
    public string SyllabusName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Effective From")]
    public DateTime EffectiveFrom { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Effective To")]
    public DateTime? EffectiveTo { get; set; }

    [MaxLength(100)]
    public string Status { get; set; } = "Active";

    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Courses")]
    public List<Guid>? CourseIds { get; set; } = new List<Guid>();
}
