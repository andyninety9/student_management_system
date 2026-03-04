using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

using SmsRazor.DAL.Repositories;

namespace SmsRazor.BLL.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IRepository<Enrollment> _enrollmentRepository;
    private readonly IRepository<Attendance> _attendanceRepository;

    public AttendanceService(
        IRepository<Enrollment> enrollmentRepository,
        IRepository<Attendance> attendanceRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _attendanceRepository = attendanceRepository;
    }

    public async Task<IEnumerable<UpcomingClassDTO>> GetStudentUpcomingClassesAsync(string studentCode, int limit = 3)
    {
        var today = DateTime.UtcNow.Date;

        var upcomingClasses = await _enrollmentRepository.Entities
            .Where(e => e.StudentCode == studentCode && e.Section!.Status == true)
            .SelectMany(e => e.Section!.Calendars)
            .Where(c => c.StudyDate >= today)
            .OrderBy(c => c.StudyDate)
            .ThenBy(c => c.Slot)
            .Take(limit)
            .Select(c => new UpcomingClassDTO
            {
                CalendarId = c.AcademicCalendarId,
                CourseName = c.Section!.Course!.CourseNameEng,
                SectionCode = c.Section.SectionCode,
                Room = "TBA",
                Date = c.StudyDate,
                Slot = c.Slot,
                Status = _attendanceRepository.Entities
                    .Where(a => a.AcademicCalendarId == c.AcademicCalendarId && a.StudentCode == studentCode)
                    .Select(a => a.Status)
                    .FirstOrDefault() // Will default to 0 (NotYetMarked) if no record exists
            })
            .ToListAsync();

        return upcomingClasses;
    }

    public async Task<IEnumerable<AttendanceRecordDTO>> GetStudentAttendanceRecordsAsync(string studentCode, Guid termId)
    {
        // Get all enrollments for the student in the given term
        var enrollments = await _enrollmentRepository.Entities
            .Include(e => e.Section!)
            .ThenInclude(s => s.Course)
            .Include(e => e.Section!.TeacherAssignment)
            .ThenInclude(ta => ta!.TeacherInfo)
            .ThenInclude(ti => ti!.Account)
            .Include(e => e.Section!.Calendars.Where(c => c.TermId == termId))
            .Where(e => e.StudentCode == studentCode && e.Section!.Calendars.Any(c => c.TermId == termId))
            .ToListAsync();

        var attendanceData = new List<AttendanceRecordDTO>();

        foreach (var enrollment in enrollments)
        {
            var section = enrollment.Section;
            var calendars = section!.Calendars.OrderBy(c => c.StudyDate).ThenBy(c => c.Slot).ToList();

            var calendarIds = calendars.Select(c => c.AcademicCalendarId).ToList();

            // Fetch actual attendance records for this mathing student and section calendars
            var attendanceRecords = await _attendanceRepository.Entities
                .Where(a => a.StudentCode == studentCode && calendarIds.Contains(a.AcademicCalendarId))
                .ToDictionaryAsync(a => a.AcademicCalendarId);

            var recordDto = new AttendanceRecordDTO
            {
                CourseName = section.Course!.CourseNameEng,
                SectionCode = section.SectionCode,
                TotalSessions = calendars.Count,
                Details = new List<AttendanceDetailDTO>()
            };

            foreach (var cal in calendars)
            {
                attendanceRecords.TryGetValue(cal.AcademicCalendarId, out var attendance);
                var status = attendance?.Status ?? AttendanceStatus.NotYetMarked;
                
                if (status == AttendanceStatus.Present) recordDto.AttendedSessions++;
                if (status == AttendanceStatus.Absent) recordDto.AbsentSessions++;

                recordDto.Details.Add(new AttendanceDetailDTO
                {
                    Date = cal.StudyDate,
                    Slot = cal.Slot,
                    Room = "TBA",
                    Teacher = section.TeacherAssignment?.TeacherInfo?.Account?.Fullname ?? "N/A",
                    Status = status,
                    Remarks = attendance?.Remarks
                });
            }

            attendanceData.Add(recordDto);
        }

        return attendanceData;
    }
}
