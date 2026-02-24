using System;
using System.ComponentModel.DataAnnotations;

namespace SmsRazor.DAL.Entities;

public class Course : BaseEntity
{
    [Key]
    public Guid CourseId { get; set; }

    [Required]
    [MaxLength(255)]
    public string CourseNameEng { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string CourseNameVI { get; set; } = string.Empty;

    public int CreditNumber { get; set; }

    public bool IsActive { get; set; } = true;

    // Optional Foreign Keys mapping to ERD
    public Guid? CourseRequiredID { get; set; }

    // Navigation for Many-to-Many Syllabus
    public ICollection<SyllabusCourse> SyllabusCourses { get; set; } = new List<SyllabusCourse>();

    // Navigation for Prerequisites
    public ICollection<CoursePrerequisite> Prerequisites { get; set; } = new List<CoursePrerequisite>();
    public ICollection<CoursePrerequisite> PrerequisiteFor { get; set; } = new List<CoursePrerequisite>();
}
