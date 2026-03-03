using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

[Table("Section")]
public class Section : BaseEntity
{
    [Key]
    public Guid SectionId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("sectionCode")]
    public string SectionCode { get; set; } = string.Empty;

    public Guid? TeacherAssignmentId { get; set; }

    [Column("capacity")]
    public int Capacity { get; set; }

    [Column("status")]
    public bool Status { get; set; } = true;

    // Navigation Properties
    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }

    [ForeignKey(nameof(TeacherAssignmentId))]
    public TeacherAssignment? TeacherAssignment { get; set; }

    public ICollection<AcademicCalendar> Calendars { get; set; } = new List<AcademicCalendar>();
}
