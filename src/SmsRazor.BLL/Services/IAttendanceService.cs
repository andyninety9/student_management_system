using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsRazor.DAL.Entities;

namespace SmsRazor.BLL.Services;

public interface IAttendanceService
{
    Task<IEnumerable<UpcomingClassDTO>> GetStudentUpcomingClassesAsync(string studentCode, int limit = 3);
    Task<IEnumerable<AttendanceRecordDTO>> GetStudentAttendanceRecordsAsync(string studentCode, Guid termId);
}

public class UpcomingClassDTO
{
    public Guid CalendarId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Slot { get; set; }
    public AttendanceStatus Status { get; set; }
}

public class AttendanceRecordDTO
{
    public string CourseName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int AttendedSessions { get; set; }
    public int AbsentSessions { get; set; }
    public double AbsencePercentage => TotalSessions == 0 ? 0 : Math.Round((double)AbsentSessions / TotalSessions * 100, 2);
    public List<AttendanceDetailDTO> Details { get; set; } = new();
}

public class AttendanceDetailDTO
{
    public DateTime Date { get; set; }
    public int Slot { get; set; }
    public string Room { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
}
