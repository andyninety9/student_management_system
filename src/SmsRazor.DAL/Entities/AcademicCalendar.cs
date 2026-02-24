using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

[Table("AcademicCalendar")]
public class AcademicCalendar : BaseEntity
{
    [Key]
    public Guid AcademicCalendarId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("termID")]
    public Guid TermId { get; set; }

    [Required]
    [Column("sectionID")]
    public Guid SectionId { get; set; }

    // According to the image, it has start_date, end_date? Wait, let me check the image again or keep my own logic from plan.
    // Wait, the plan said "StudyDate" and "Slot". Let me match the plan, since the DB schema only said "AcademicCalendar", "TermID", "SectionID". It didn't expand columns.
    // I will add columns for StudyDate and Slot to make timetables work.
    
    [Column("study_date")]
    public DateTime StudyDate { get; set; }

    [Column("slot")]
    public int Slot { get; set; }

    [Column("status")]
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    [ForeignKey(nameof(TermId))]
    public Term? Term { get; set; }

    [ForeignKey(nameof(SectionId))]
    public Section? Section { get; set; }
}
