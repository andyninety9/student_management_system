using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Student.Timetable
{
    [Authorize(Roles = "Student")]
    public class IndexModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ISectionService _sectionService;

        public IndexModel(IEnrollmentService enrollmentService, ISectionService sectionService)
        {
            _enrollmentService = enrollmentService;
            _sectionService = sectionService;
        }

        public string StudentCode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public Guid SelectedTermId { get; set; }
        public SelectList TermsList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var studentCodeClaim = User.FindFirst("StudentCode")?.Value;
            StudentCode = studentCodeClaim ?? "";

            var termsLookup = await _sectionService.GetTermsLookupAsync();
            TermsList = new SelectList(termsLookup, "Key", "Value");

            return Page();
        }

        public async Task<IActionResult> OnGetStudentTimetableAsync(string studentCode, Guid termId)
        {
            if (string.IsNullOrEmpty(studentCode) || termId == Guid.Empty)
                return new JsonResult(new List<StudentScheduleDTO>());

            var events = await _enrollmentService.GetStudentTimetableAsync(studentCode, termId);
            return new JsonResult(events);
        }
    }
}
