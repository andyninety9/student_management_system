using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

public class SyllabusCourse : BaseEntity
{
    [Key]
    public Guid SyllabusCourseId { get; set; }

    [Required]
    public Guid SyllabusId { get; set; }

    [ForeignKey("SyllabusId")]
    public Syllabus? Syllabus { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [ForeignKey("CourseId")]
    public Course? Course { get; set; }
}
