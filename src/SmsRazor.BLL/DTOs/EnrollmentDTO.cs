using System;
using System.ComponentModel.DataAnnotations;

namespace SmsRazor.BLL.DTOs;

public class EnrollmentDTO
{
    public Guid EnrollmentId { get; set; }

    public string StudentCode { get; set; } = string.Empty;

    public Guid SectionId { get; set; }
    
    public string SectionCode { get; set; } = string.Empty;

    public Guid CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public int? Credits { get; set; }

    public string TeacherName { get; set; } = string.Empty;

    public DateTime EnrollmentDate { get; set; }
}
