using System;
using System.ComponentModel.DataAnnotations;

namespace SmsRazor.BLL.DTOs;

public class CourseDTO
{
    public Guid CourseId { get; set; }

    [Required]
    [MaxLength(255)]
    [Display(Name = "English Name")]
    public string CourseNameEng { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Display(Name = "Vietnamese Name")]
    public string CourseNameVI { get; set; } = string.Empty;

    [Required]
    [Range(1, 10)]
    [Display(Name = "Credits")]
    public int CreditNumber { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Required Course")]
    public Guid? CourseRequiredID { get; set; }

    [Display(Name = "Prerequisites")]
    public List<Guid>? PrerequisiteCourseIds { get; set; } = new List<Guid>();

    public bool HasAvailableSections { get; set; }
}
