using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

public class CoursePrerequisite : BaseEntity
{
    [Key]
    public Guid CoursePrerequisiteId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [ForeignKey("CourseId")]
    public Course? Course { get; set; }

    [Required]
    public Guid PrerequisiteCourseId { get; set; }

    [ForeignKey("PrerequisiteCourseId")]
    public Course? PrerequisiteCourse { get; set; }
}
