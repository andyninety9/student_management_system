using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmsRazor.DAL.Data;

namespace SmsRazor.WebApp.Pages.Teacher;

[Authorize(Roles = "Teacher")]
public class IndexModel : PageModel
{
    private readonly SmsDbContext _dbContext;

    public IndexModel(SmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }
    public List<dynamic> UpcomingClasses { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var teacherAccountId))
        {
            return RedirectToPage("/Auth/Login");
        }

        // Fetch Total Active Classes
        TotalClasses = await _dbContext.Sections
            .Include(s => s.TeacherAssignment)
            .ThenInclude(ta => ta.TeacherInfo)
            .Where(s => s.TeacherAssignment != null && s.TeacherAssignment.TeacherInfo != null && s.TeacherAssignment.TeacherInfo.AccountId == teacherAccountId && s.Status)
            .CountAsync();

        // Fetch Total distinct Students
        TotalStudents = await _dbContext.Enrollments
            .Include(e => e.Section)
            .ThenInclude(s => s.TeacherAssignment)
            .ThenInclude(ta => ta.TeacherInfo)
            .Where(e => e.Section.TeacherAssignment != null && e.Section.TeacherAssignment.TeacherInfo != null && e.Section.TeacherAssignment.TeacherInfo.AccountId == teacherAccountId && e.Section.Status)
            .Select(e => e.StudentCode)
            .Distinct()
            .CountAsync();

        // Fetch upcoming 3 classes from today
        var today = DateTime.UtcNow;
        var upcoming = await _dbContext.AcademicCalendars
            .Include(a => a.Section)
            .ThenInclude(s => s.Course)
            .Include(a => a.Section.TeacherAssignment)
            .ThenInclude(ta => ta.TeacherInfo)
            .Where(a => a.Section.TeacherAssignment != null && a.Section.TeacherAssignment.TeacherInfo != null && a.Section.TeacherAssignment.TeacherInfo.AccountId == teacherAccountId && a.StudyDate >= today)
            .OrderBy(a => a.StudyDate)
            .Take(4)
            .ToListAsync();
            
        foreach(var item in upcoming)
        {
            // Note: Section doesn't have SectionName or Room according to schema, let's use SectionCode and hardcode "TBD" for room
            UpcomingClasses.Add(new {
                CourseCode = item.Section?.Course?.CourseNameEng,
                SectionName = item.Section?.SectionCode,
                Date = item.StudyDate,
                Slot = item.Slot,
                Room = "TBD"
            });
        }

        return Page();
    }
}
