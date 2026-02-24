using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Courses
{
    [Authorize(Roles = "Admin,Root")]
    public class EditModel : PageModel
    {
        private readonly ICourseService _courseService;

        public EditModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [BindProperty]
        public CourseDTO Course { get; set; } = new CourseDTO();

        public MultiSelectList CoursesList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _courseService.GetCourseByIdAsync(id.Value);

            if (course == null)
            {
                return NotFound();
            }

            Course = course;
            
            var courses = await _courseService.GetCoursesLookupAsync();
            // Filter out the current course to prevent self-prerequisite dependency
            var availableCourses = courses.Where(c => c.Key != id.Value);
            CoursesList = new MultiSelectList(availableCourses, "Key", "Value");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var courses = await _courseService.GetCoursesLookupAsync();
                var availableCourses = courses.Where(c => c.Key != Course.CourseId);
                CoursesList = new MultiSelectList(availableCourses, "Key", "Value");
                
                return Page();
            }

            var success = await _courseService.UpdateCourseAsync(Course);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToPage("./Index");
        }
    }
}
