using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmsRazor.BLL.DTOs;

public class SectionDTO
{
    public Guid SectionId { get; set; }
    
    [Required]
    [Display(Name = "Section Code")]
    public string SectionCode { get; set; } = string.Empty;

    public Guid CourseId { get; set; }
    public string? CourseName { get; set; }

    public Guid? TeacherAssignmentId { get; set; }
    public string? TeacherCode { get; set; }
    public string? TeacherName { get; set; }

    [Display(Name = "Capacity")]
    public int Capacity { get; set; }

    [Display(Name = "Status")]
    public bool Status { get; set; }

    // Display fields for easy listing
    public int TotalSessions { get; set; }
    public string? TermName { get; set; }
    public string? ScheduleTime { get; set; }
}

public class ScheduleGenerationDTO
{
    [Required]
    [Display(Name = "Term")]
    public Guid TermId { get; set; }

    [Required]
    [Display(Name = "Course")]
    public Guid CourseId { get; set; }

    [Required]
    [Display(Name = "Section/Class Code")]
    public string SectionCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Primary Teacher")]
    public string TeacherCode { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Capacity { get; set; } = 30;

    [Range(1, 16)]
    [Display(Name = "Duration (Weeks)")]
    public int DurationWeeks { get; set; } = 8;

    [Display(Name = "Study Schedules")]
    public List<ScheduleDayOption> ScheduleDays { get; set; } = new List<ScheduleDayOption>();
}

public class ScheduleDayOption
{
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsSelected { get; set; }
    
    [Display(Name = "Time Slot")]
    public int Slot { get; set; } = 1; // e.g. 1 to 6
}
