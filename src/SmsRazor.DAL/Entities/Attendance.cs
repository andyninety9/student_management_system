using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

public enum AttendanceStatus
{
    NotYetMarked = 0,
    Present = 1,
    Absent = 2,
    Excused = 3
}

public class Attendance : BaseEntity
{
    [Key]
    public Guid AttendanceId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(20)]
    public string StudentCode { get; set; } = string.Empty;

    public Guid AcademicCalendarId { get; set; }

    [Required]
    public AttendanceStatus Status { get; set; } = AttendanceStatus.NotYetMarked;

    [MaxLength(255)]
    public string? Remarks { get; set; }

    // Navigation properties
    [ForeignKey("StudentCode")]
    public virtual StudentInfo Student { get; set; } = null!;

    [ForeignKey("AcademicCalendarId")]
    public virtual AcademicCalendar Calendar { get; set; } = null!;
}
