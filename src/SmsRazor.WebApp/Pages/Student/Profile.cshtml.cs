using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class ProfileModel : PageModel
    {
        private readonly IStudentService _studentService;

        public ProfileModel(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [BindProperty]
        public StudentProfileUpdateDTO Input { get; set; } = new();

        public StudentDTO? ReadonlyProfile { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !Guid.TryParse(accountIdClaim, out var accountId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var student = await _studentService.GetStudentByIdAsync(accountId);
            if (student == null)
            {
                return NotFound("Student profile not found.");
            }

            StudentName = student.Fullname;
            StudentCode = student.StudentCode;
            ReadonlyProfile = student;

            Input = new StudentProfileUpdateDTO
            {
                AccountId = accountId,
                Phone = student.Phone,
                Dob = student.Dob,
                Gender = student.Gender,
                Address = student.Address
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !Guid.TryParse(accountIdClaim, out var accountId))
            {
                return RedirectToPage("/Auth/Login");
            }
            
            // Reload ReadonlyProfile if post fails
            var student = await _studentService.GetStudentByIdAsync(accountId);
            if (student != null)
            {
                StudentName = student.Fullname;
                StudentCode = student.StudentCode;
                ReadonlyProfile = student;
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Security check: Only update the current logged in user's ID
            if (Input.AccountId != accountId)
            {
                StatusMessage = "Error: Invalid operation.";
                return Page();
            }

            var success = await _studentService.UpdateStudentProfileAsync(accountId, Input);
            if (success)
            {
                StatusMessage = "Success: Your profile has been updated successfully.";
                return RedirectToPage();
            }

            StatusMessage = "Error: Failed to update your profile. Please contact Support.";
            return Page();
        }
    }
}
