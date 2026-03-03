using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsRazor.BLL.DTOs;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

namespace SmsRazor.BLL.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly SmsDbContext _context;

    public EnrollmentService(SmsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CourseDTO>> GetStudentSyllabusCoursesAsync(string studentCode, Guid? termId = null)
    {
        var student = await _context.StudentInfos
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentCode == studentCode);

        if (student == null) return new List<CourseDTO>();

        var query = _context.SyllabusCourses
            .Include(sc => sc.Course)
            .Where(sc => sc.SyllabusId == student.SyllabusId);

        if (termId.HasValue)
        {
            var tid = termId.Value;
            var syllabusCourses = await query
                .Select(sc => new CourseDTO
                {
                    CourseId = sc.Course!.CourseId,
                    CourseNameEng = sc.Course.CourseNameEng,
                    CourseNameVI = sc.Course.CourseNameVI,
                    CreditNumber = sc.Course.CreditNumber,
                    IsActive = sc.Course.IsActive,
                    HasAvailableSections = _context.Sections.Any(s => s.CourseId == sc.Course.CourseId && s.Status == true && s.Calendars.Any(c => c.TermId == tid) && (s.Capacity - _context.Enrollments.Count(e => e.SectionId == s.SectionId)) > 0)
                })
                .ToListAsync();

            return syllabusCourses;
        }
        else
        {
            var syllabusCourses = await query
                .Select(sc => new CourseDTO
                {
                    CourseId = sc.Course!.CourseId,
                    CourseNameEng = sc.Course.CourseNameEng,
                    CourseNameVI = sc.Course.CourseNameVI,
                    CreditNumber = sc.Course.CreditNumber,
                    IsActive = sc.Course.IsActive,
                    HasAvailableSections = false
                })
                .ToListAsync();

            return syllabusCourses;
        }
    }

    public async Task<IEnumerable<SectionDTO>> GetAvailableSectionsForCourseAsync(Guid courseId, Guid termId)
    {
        var sections = await _context.Sections
            .Include(s => s.TeacherAssignment)
            .Include(s => s.TeacherAssignment!.TeacherInfo!.Account)
            .Include(s => s.Calendars)
            .Where(s => s.CourseId == courseId && s.Status == true && s.Calendars.Any(c => c.TermId == termId))
            .Select(s => new
            {
                Section = s,
                TeacherName = s.TeacherAssignment!.TeacherInfo!.Account!.Fullname,
                EnrollmentCount = _context.Enrollments.Count(e => e.SectionId == s.SectionId),
                Calendars = s.Calendars.Where(c => c.TermId == termId).ToList()
            })
            .ToListAsync();

        var availableSections = sections.Select(x => {
            var schedules = x.Calendars
                .Select(c => new { Day = c.StudyDate.DayOfWeek, c.Slot })
                .Distinct()
                .OrderBy(d => d.Day == DayOfWeek.Sunday ? 7 : (int)d.Day)
                .Select(d => $"{d.Day.ToString().Substring(0, 3)} (Slot {d.Slot})");

            return new SectionDTO
            {
                SectionId = x.Section.SectionId,
                CourseId = x.Section.CourseId,
                SectionCode = x.Section.SectionCode,
                TeacherAssignmentId = x.Section.TeacherAssignmentId,
                TeacherName = string.IsNullOrEmpty(x.TeacherName) ? "TBA" : x.TeacherName,
                Capacity = x.Section.Capacity - x.EnrollmentCount,
                Status = x.Section.Status,
                ScheduleTime = string.Join(", ", schedules)
            };
        })
        .Where(s => s.Capacity > 0)
        .ToList();

        return availableSections;
    }

    public async Task<IEnumerable<EnrollmentDTO>> GetStudentEnrollmentsByTermAsync(string studentCode, Guid termId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Section).ThenInclude(s => s!.Course)
            .Include(e => e.Section).ThenInclude(s => s!.TeacherAssignment).ThenInclude(ta => ta!.TeacherInfo).ThenInclude(ti => ti!.Account)
            // Join with Calendars to filter by Term
            .Where(e => e.StudentCode == studentCode && e.Section!.Calendars.Any(c => c.TermId == termId))
            .Select(e => new EnrollmentDTO
            {
                EnrollmentId = e.EnrollmentId,
                SectionId = e.SectionId,
                StudentCode = e.StudentCode,
                SectionCode = e.Section!.SectionCode,
                CourseId = e.Section.CourseId,
                CourseName = e.Section.Course != null ? e.Section.Course.CourseNameEng : "",
                Credits = e.Section.Course != null ? e.Section.Course.CreditNumber : 0,
                TeacherName = e.Section.TeacherAssignment != null && e.Section.TeacherAssignment.TeacherInfo != null && e.Section.TeacherAssignment.TeacherInfo.Account != null
                        ? e.Section.TeacherAssignment.TeacherInfo.Account.Fullname ?? "TBA" : "TBA",
                EnrollmentDate = e.EnrollmentDate
            })
            .ToListAsync();

        return enrollments;
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> RegisterForSectionAsync(string studentCode, Guid sectionId)
    {
        // 1. Get the section and determine its Term
        var section = await _context.Sections
            .Include(s => s.Calendars)
            .FirstOrDefaultAsync(s => s.SectionId == sectionId);

        if (section == null || !section.Status)
            return (false, "Section not found or inactive.");

        var termId = section.Calendars.FirstOrDefault()?.TermId;
        if (termId == null)
            return (false, "Section is not properly scheduled in any Term.");

        // 2. Check Capacity
        var currentEnrollmentsCount = await _context.Enrollments.CountAsync(e => e.SectionId == sectionId);
        if (currentEnrollmentsCount >= section.Capacity)
            return (false, "This section is already full.");

        // 3. Check exact duplicates
        var exists = await _context.Enrollments.AnyAsync(e => e.StudentCode == studentCode && e.SectionId == sectionId);
        if (exists)
            return (false, "You are already registered for this section.");

        // 4. Fetch student's current enrollments for THIS term (to check max 5, and duplicate courses)
        var studentTermEnrollments = await _context.Enrollments
            .Include(e => e.Section)
            .ThenInclude(s => s!.Calendars)
            .Where(e => e.StudentCode == studentCode && e.Section!.Calendars.Any(c => c.TermId == termId))
            .ToListAsync();

        // Check Max 5 limit
        if (studentTermEnrollments.Count >= 5)
            return (false, "You have reached the maximum limit of 5 courses for this term.");

        // Check Duplicate Course rule: Cannot register for the matching Course in the same term
        foreach (var e in studentTermEnrollments)
        {
            if (e.Section?.CourseId == section.CourseId)
            {
                return (false, "You are already registered for a different section of this same Course.");
            }
        }

        // 5. Save Registration
        var enrollment = new Enrollment
        {
            StudentCode = studentCode,
            SectionId = sectionId,
            EnrollmentDate = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return (true, "Registered successfully.");
    }

    public async Task<bool> RemoveRegistrationAsync(string studentCode, Guid sectionId)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentCode == studentCode && e.SectionId == sectionId);

        if (enrollment == null) return false;

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<StudentScheduleDTO>> GetStudentTimetableAsync(string studentCode, Guid termId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Section).ThenInclude(s => s!.Course)
            .Include(e => e.Section).ThenInclude(s => s!.Calendars)
            .Where(e => e.StudentCode == studentCode && e.Section!.Calendars.Any(c => c.TermId == termId))
            .ToListAsync();

        var events = new List<StudentScheduleDTO>();
        foreach (var e in enrollments)
        {
            if (e.Section == null || e.Section.Calendars == null) continue;
            
            // Filter calendars specifically for the requested Term (though the join above sort of ensures it, Sections can theoretically span terms)
            var termCalendars = e.Section.Calendars.Where(c => c.TermId == termId);
            
            foreach (var c in termCalendars)
            {
                var dateStr = c.StudyDate.ToString("yyyy-MM-dd");
                string startTime = "00:00:00";
                string endTime = "23:59:59";
                
                switch (c.Slot)
                {
                    case 1: startTime = "07:00:00"; endTime = "09:15:00"; break;
                    case 2: startTime = "09:30:00"; endTime = "11:45:00"; break;
                    case 3: startTime = "12:30:00"; endTime = "14:45:00"; break;
                    case 4: startTime = "15:00:00"; endTime = "17:15:00"; break;
                    case 5: startTime = "17:30:00"; endTime = "19:45:00"; break;
                    case 6: startTime = "20:00:00"; endTime = "22:15:00"; break;
                }

                events.Add(new StudentScheduleDTO
                {
                    title = $"{e.Section.SectionCode} - {e.Section.Course?.CourseNameEng}",
                    start = $"{dateStr}T{startTime}",
                    end = $"{dateStr}T{endTime}",
                    className = "bg-success bg-opacity-75 text-white border-0 rounded-2 px-2 py-1 shadow-sm fs-7"
                });
            }
        }
        
        return events;
    }
}
