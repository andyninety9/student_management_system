using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Teachers
{
    [Authorize(Roles = "Admin,Root")]
    public class DeleteModel : PageModel
    {
        private readonly ITeacherService _teacherService;

        public DeleteModel(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [BindProperty]
        public TeacherDTO Teacher { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var teacher = await _teacherService.GetTeacherByCodeAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            Teacher = teacher;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Teacher.TeacherCode))
            {
                return NotFound();
            }

            var success = await _teacherService.DeleteTeacherAsync(Teacher.TeacherCode);
            if (success)
            {
                StatusMessage = $"Success: Teacher '{Teacher.TeacherCode}' was deleted successfully.";
            }
            else
            {
                StatusMessage = $"Error: Failed to delete teacher '{Teacher.TeacherCode}'.";
            }

            return RedirectToPage("./Index");
        }
    }
}
