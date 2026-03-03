using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Teachers
{
    [Authorize(Roles = "Admin,Root")]
    public class IndexModel : PageModel
    {
        private readonly ITeacherService _teacherService;
        private readonly ISectionService _sectionService;

        public IndexModel(ITeacherService teacherService, ISectionService sectionService)
        {
            _teacherService = teacherService;
            _sectionService = sectionService;
        }

        public IEnumerable<TeacherDTO> Teachers { get; set; } = new List<TeacherDTO>();
        [BindProperty]
        public Guid SelectedTermId { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList TermsList { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Teachers = await _teacherService.GetAllTeachersAsync();
            var termsLookup = await _sectionService.GetTermsLookupAsync();
            TermsList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(termsLookup, "Key", "Value");
            return Page();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(string teacherCode, string newPassword)
        {
            if (string.IsNullOrEmpty(teacherCode) || string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                StatusMessage = "Error: Invalid input for password reset.";
                return RedirectToPage();
            }

            var success = await _teacherService.ResetTeacherPasswordAsync(teacherCode, newPassword);
            if (success)
            {
                StatusMessage = $"Success: Password for {teacherCode} has been reset.";
            }
            else
            {
                StatusMessage = $"Error: Failed to reset password for {teacherCode}.";
            }

            return RedirectToPage();
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
