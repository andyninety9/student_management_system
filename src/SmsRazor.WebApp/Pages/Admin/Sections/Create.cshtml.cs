using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Sections
{
    [Authorize(Roles = "Admin,Root")]
    public class CreateModel : PageModel
    {
        private readonly ISectionService _sectionService;

        public CreateModel(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [BindProperty]
        public ScheduleGenerationDTO ScheduleGen { get; set; } = new ScheduleGenerationDTO();

        public SelectList TermsList { get; set; } = default!;
        public SelectList CoursesList { get; set; } = default!;
        public SelectList TeachersList { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            InitializeScheduleOptions();
            await PopulateDropDownsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync();
                return Page();
            }

            try
            {
                var sectionId = await _sectionService.CreateSectionAndScheduleAsync(ScheduleGen);
                StatusMessage = $"Success: Class '{ScheduleGen.SectionCode}' created and timetables correctly scheduled.";
                return RedirectToPage("./Timetable", new { id = sectionId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropDownsAsync();
                return Page();
            }
        }

        private void InitializeScheduleOptions()
        {
            if (!ScheduleGen.ScheduleDays.Any())
            {
                ScheduleGen.ScheduleDays = new List<ScheduleDayOption>
                {
                    new ScheduleDayOption { DayOfWeek = DayOfWeek.Monday, IsSelected = false },
                    new ScheduleDayOption { DayOfWeek = DayOfWeek.Tuesday, IsSelected = false },
                    new ScheduleDayOption { DayOfWeek = DayOfWeek.Wednesday, IsSelected = false },
                    new ScheduleDayOption { DayOfWeek = DayOfWeek.Thursday, IsSelected = false },
                    new ScheduleDayOption { DayOfWeek = DayOfWeek.Friday, IsSelected = false },
                    new ScheduleDayOption { DayOfWeek = DayOfWeek.Saturday, IsSelected = false },
                    new ScheduleDayOption { DayOfWeek = DayOfWeek.Sunday, IsSelected = false },
                };
            }
        }

        private async Task PopulateDropDownsAsync()
        {
            var terms = await _sectionService.GetTermsLookupAsync();
            var courses = await _sectionService.GetCoursesLookupAsync();
            var teachers = await _sectionService.GetTeachersLookupAsync();

            TermsList = new SelectList(terms, "Key", "Value");
            CoursesList = new SelectList(courses, "Key", "Value");
            TeachersList = new SelectList(teachers, "Key", "Value");
        }

        public async Task<IActionResult> OnGetTeacherScheduleAsync(string teacherCode, Guid termId)
        {
            if (string.IsNullOrEmpty(teacherCode) || termId == Guid.Empty)
            {
                return new JsonResult(new List<TeacherScheduleDTO>());
            }
            
            var events = await _sectionService.GetTeacherScheduleAsync(teacherCode, termId);
            return new JsonResult(events);
        }
    }
}
