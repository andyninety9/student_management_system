using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Students
{
    [Authorize(Roles = "Admin,Root")]
    public class DeleteModel : PageModel
    {
        private readonly IStudentService _studentService;

        public DeleteModel(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [BindProperty]
        public StudentDTO Student { get; set; } = new StudentDTO();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var student = await _studentService.GetStudentByCodeAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            Student = student;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Student.StudentCode))
            {
                return NotFound();
            }

            var success = await _studentService.DeleteStudentAsync(Student.StudentCode);
            if (!success)
            {
                return NotFound();
            }

            TempData["StatusMessage"] = $"Success: Student {Student.StudentCode} has been deleted.";
            return RedirectToPage("./Index");
        }
    }
}
