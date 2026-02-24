using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Courses
{
    [Authorize(Roles = "Admin,Root")]
    public class DeleteModel : PageModel
    {
        private readonly ICourseService _courseService;

        public DeleteModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [BindProperty]
        public CourseDTO Course { get; set; } = default!;

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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var success = await _courseService.DeleteCourseAsync(id.Value);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToPage("./Index");
        }
    }
}
