using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Students
{
    [Authorize(Roles = "Admin,Root")]
    public class IndexModel : PageModel
    {
        private readonly IStudentService _studentService;

        public IndexModel(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public IEnumerable<StudentDTO> Students { get; set; } = new List<StudentDTO>();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Students = await _studentService.GetAllStudentsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(string studentCode, string newPassword)
        {
            if (string.IsNullOrEmpty(studentCode) || string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                StatusMessage = "Error: Invalid input for password reset.";
                return RedirectToPage();
            }

            var success = await _studentService.ResetStudentPasswordAsync(studentCode, newPassword);
            if (success)
            {
                StatusMessage = $"Success: Password for {studentCode} has been reset.";
            }
            else
            {
                StatusMessage = $"Error: Failed to reset password for {studentCode}.";
            }

            return RedirectToPage();
        }
    }
}
