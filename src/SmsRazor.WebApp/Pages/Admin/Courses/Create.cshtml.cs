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
    public class CreateModel : PageModel
    {
        private readonly ICourseService _courseService;

        public CreateModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [BindProperty]
        public CourseDTO Course { get; set; } = new CourseDTO();

        public MultiSelectList CoursesList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            var courses = await _courseService.GetCoursesLookupAsync();
            CoursesList = new MultiSelectList(courses, "Key", "Value");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var courses = await _courseService.GetCoursesLookupAsync();
                CoursesList = new MultiSelectList(courses, "Key", "Value");
                return Page();
            }

            await _courseService.CreateCourseAsync(Course);
            return RedirectToPage("./Index");
        }
    }
}
