using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsRazor.BLL.DTOs;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

using SmsRazor.DAL.Repositories;

namespace SmsRazor.BLL.Services;

public class SectionService : ISectionService
{
    private readonly IRepository<Section> _sectionRepository;
    private readonly IRepository<Term> _termRepository;
    private readonly IRepository<Course> _courseRepository;
    private readonly IRepository<TeacherInfo> _teacherInfoRepository;
    private readonly IRepository<TeacherAssignment> _teacherAssignmentRepository;
    private readonly IRepository<AcademicCalendar> _academicCalendarRepository;

    public SectionService(
        IRepository<Section> sectionRepository,
        IRepository<Term> termRepository,
        IRepository<Course> courseRepository,
        IRepository<TeacherInfo> teacherInfoRepository,
        IRepository<TeacherAssignment> teacherAssignmentRepository,
        IRepository<AcademicCalendar> academicCalendarRepository)
    {
        _sectionRepository = sectionRepository;
        _termRepository = termRepository;
        _courseRepository = courseRepository;
        _teacherInfoRepository = teacherInfoRepository;
        _teacherAssignmentRepository = teacherAssignmentRepository;
        _academicCalendarRepository = academicCalendarRepository;
    }

    public async Task<IEnumerable<SectionDTO>> GetAllSectionsAsync()
    {
        var sections = await _sectionRepository.Entities
            .Include(s => s.Course)
            .Include(s => s.TeacherAssignment).ThenInclude(ta => ta!.TeacherInfo).ThenInclude(ti => ti!.Account)
            .Include(s => s.Calendars).ThenInclude(c => c.Term)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return sections.Select(s => new SectionDTO
        {
            SectionId = s.SectionId,
            SectionCode = s.SectionCode,
            CourseId = s.CourseId,
            CourseName = s.Course?.CourseNameEng,
            TeacherAssignmentId = s.TeacherAssignmentId,
            TeacherCode = s.TeacherAssignment?.TeacherCode,
            TeacherName = s.TeacherAssignment?.TeacherInfo?.Account?.Fullname,
            Capacity = s.Capacity,
            Status = s.Status,
            TotalSessions = s.Calendars.Count,
            TermName = s.Calendars.FirstOrDefault()?.Term?.Name
        });
    }

    public async Task<SectionDTO?> GetSectionByIdAsync(Guid sectionId)
    {
        var s = await _sectionRepository.Entities
            .Include(s => s.Course)
            .Include(s => s.TeacherAssignment).ThenInclude(ta => ta!.TeacherInfo).ThenInclude(ti => ti!.Account)
            .Include(s => s.Calendars).ThenInclude(c => c.Term)
            .FirstOrDefaultAsync(x => x.SectionId == sectionId);

        if (s == null) return null;

        return new SectionDTO
        {
            SectionId = s.SectionId,
            SectionCode = s.SectionCode,
            CourseId = s.CourseId,
            CourseName = s.Course?.CourseNameEng,
            TeacherAssignmentId = s.TeacherAssignmentId,
            TeacherCode = s.TeacherAssignment?.TeacherCode,
            TeacherName = s.TeacherAssignment?.TeacherInfo?.Account?.Fullname,
            Capacity = s.Capacity,
            Status = s.Status,
            TotalSessions = s.Calendars.Count,
            TermName = s.Calendars.FirstOrDefault()?.Term?.Name
        };
    }

    public async Task<Guid> CreateSectionAndScheduleAsync(ScheduleGenerationDTO dto)
    {
        // 1. Basic validation
        var term = await _termRepository.GetByIdAsync(dto.TermId);
        if (term == null) throw new Exception("Invalid Term.");

        var course = await _courseRepository.GetByIdAsync(dto.CourseId);
        if (course == null) throw new Exception("Invalid Course.");

        var teacher = await _teacherInfoRepository.Entities.FirstOrDefaultAsync(t => t.TeacherCode == dto.TeacherCode);
        if (teacher == null) throw new Exception("Invalid Primary Teacher.");

        if (await _sectionRepository.Entities.AnyAsync(s => s.SectionCode == dto.SectionCode && s.CourseId == dto.CourseId))
            throw new Exception("Section code already exists for this course.");

        // 2. Create TeacherAssignment
        var assignment = new TeacherAssignment
        {
            TeacherCode = dto.TeacherCode
        };
        await _teacherAssignmentRepository.AddAsync(assignment);

        // 3. Create Section
        var sectionId = Guid.NewGuid();
        var section = new Section
        {
            SectionId = sectionId,
            SectionCode = dto.SectionCode,
            CourseId = dto.CourseId,
            TeacherAssignmentId = assignment.TeacherAssignmentId,
            Capacity = dto.Capacity,
            Status = true
        };
        await _sectionRepository.AddAsync(section);

        // 4. Generate Timetable (AcademicCalendars)
        // Ensure to preserve universal time correctly since Postgres timestamp with timezone expects UTC
        DateTime currentStudyDate = DateTime.SpecifyKind(term.StartDate.Date, DateTimeKind.Utc);
        int weeksGenerated = 0;
        
        var selectedDays = dto.ScheduleDays.Where(d => d.IsSelected).ToList();
        if (!selectedDays.Any()) throw new Exception("At least one study schedule day must be selected.");

        while (weeksGenerated < dto.DurationWeeks)
        {
            bool weekHadClass = false;

            // Loop through Mon-Sun of current week
            for (int i = 0; i < 7; i++)
            {
                var checkDate = currentStudyDate.AddDays(i);
                
                // If the checkDate goes beyond the term's end date, we could theoretically stop, 
                // but let's trust the calculation.
                if (checkDate > term.EndDate) break;

                var matchSetting = selectedDays.FirstOrDefault(d => d.DayOfWeek == checkDate.DayOfWeek);
                if (matchSetting != null)
                {
                    await _academicCalendarRepository.AddAsync(new AcademicCalendar
                    {
                        TermId = term.TermId,
                        SectionId = sectionId,
                        StudyDate = checkDate,
                        Slot = matchSetting.Slot,
                        IsActive = true
                    });
                    weekHadClass = true;
                }
            }

            // Move to next week
            currentStudyDate = currentStudyDate.AddDays(7);
            
            // Increment week count only if we actually generated classes this week
            // (e.g. if term started on Thursday, Mon/Wed classes wouldn't hit week 1, so we shift)
            if (weekHadClass)
            {
                weeksGenerated++;
            }
        }

        await _sectionRepository.SaveChangesAsync();
        return sectionId;
    }

    public async Task<bool> DeleteSectionAsync(Guid sectionId)
    {
        var section = await _sectionRepository.Entities
            .Include(s => s.TeacherAssignment)
            .Include(s => s.Calendars)
            .FirstOrDefaultAsync(s => s.SectionId == sectionId);

        if (section == null) return false;

        _academicCalendarRepository.RemoveRange(section.Calendars);
        _sectionRepository.Remove(section);
        
        if (section.TeacherAssignment != null)
        {
             _teacherAssignmentRepository.Remove(section.TeacherAssignment);
        }

        await _sectionRepository.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetTermsLookupAsync()
    {
        var terms = await _termRepository.Entities.OrderByDescending(t => t.IsActive).ThenByDescending(t => t.StartDate).ToListAsync();
        return terms.Select(t => new KeyValuePair<Guid, string>(t.TermId, t.Name + (t.IsActive ? " (Active)" : "")));
    }

    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetCoursesLookupAsync()
    {
        var courses = await _courseRepository.Entities.Where(c => c.IsActive).OrderBy(c => c.CourseNameEng).ToListAsync();
        return courses.Select(c => new KeyValuePair<Guid, string>(c.CourseId, $"{c.CourseNameEng}"));
    }

    public async Task<IEnumerable<KeyValuePair<string, string>>> GetTeachersLookupAsync()
    {
         var teachers = await _teacherInfoRepository.Entities
             .Include(t => t.Account)
             .OrderBy(t => t.TeacherCode)
             .ToListAsync();
         return teachers.Select(t => new KeyValuePair<string, string>(t.TeacherCode, $"{t.TeacherCode} - {t.Account?.Fullname}"));
    }

    public async Task<IEnumerable<TeacherScheduleDTO>> GetTeacherScheduleAsync(string teacherCode, Guid termId)
    {
        var calendars = await _academicCalendarRepository.Entities
            .Include(c => c.Section).ThenInclude(s => s!.Course)
            .Include(c => c.Section).ThenInclude(s => s!.TeacherAssignment)
            .Where(c => c.TermId == termId && c.Section != null && c.Section.TeacherAssignment != null && c.Section.TeacherAssignment.TeacherCode == teacherCode && c.IsActive)
            .ToListAsync();

        var events = new List<TeacherScheduleDTO>();
        foreach (var c in calendars)
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

            var courseCode = c.Section?.Course?.CourseNameEng ?? "Unknown";

            events.Add(new TeacherScheduleDTO
            {
                title = $"{c.Section?.SectionCode} - {courseCode}",
                start = $"{dateStr}T{startTime}",
                end = $"{dateStr}T{endTime}",
                className = "bg-primary bg-opacity-75 text-white border-0 rounded-2 px-2 py-1 shadow-sm fs-7"
            });
        }
        
        return events;
    }
}
