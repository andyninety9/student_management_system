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

namespace SmsRazor.WebApp.Pages.Student.Registration
{
    [Authorize(Roles = "Student")]
    public class IndexModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ISectionService _sectionService; // Need to reuse to fetch Terms lookup
        private readonly ITermService _termService;

        public IndexModel(IEnrollmentService enrollmentService, ISectionService sectionService, ITermService termService)
        {
            _enrollmentService = enrollmentService;
            _sectionService = sectionService;
            _termService = termService;
        }

        public string StudentCode { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public Guid SelectedTermId { get; set; }
        public SelectList TermsList { get; set; } = default!;
        
        public string TermStartDateIso { get; set; } = string.Empty;

        public IEnumerable<CourseDTO> SyllabusCourses { get; set; } = new List<CourseDTO>();
        public IEnumerable<EnrollmentDTO> CurrentEnrollments { get; set; } = new List<EnrollmentDTO>();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var studentCodeClaim = User.FindFirst("StudentCode")?.Value;
            
            // Re-fetch mapping if StudentCode claim is not present. 
            // In LoginModel we only stored AccountId. Let's fetch it if missing.
            if (string.IsNullOrEmpty(studentCodeClaim))
            {
                var accountIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(accountIdStr) && Guid.TryParse(accountIdStr, out var accountId))
                {
                    // For brevity, you can fetch the StudentCode from DB here, but let's assume 
                    // a clean approach where we might need to query the BLL.
                    // To avoid circular or long dependencies here's a direct hook if needed:
                }
                // Assuming you have a way to securely get the StudentCode..
                // For this demo, let's assume we can fetch it via another service or we add it to the Claims.
                // WE MUST ADD StudentCode TO LOGIN CLAIMS. I will add it via AccountService / LoginModel later.
            }
            // For now let's just use a placeholder to ensure the page renders until the auth flow is patched.
            StudentCode = studentCodeClaim ?? "";

            // Populate Terms Dropdown
            var termsLookup = await _sectionService.GetTermsLookupAsync();
            TermsList = new SelectList(termsLookup, "Key", "Value");

            if (!string.IsNullOrEmpty(StudentCode))
            {
                SyllabusCourses = await _enrollmentService.GetStudentSyllabusCoursesAsync(StudentCode, SelectedTermId != Guid.Empty ? SelectedTermId : null);

                if (SelectedTermId != Guid.Empty)
                {
                    var term = await _termService.GetTermByIdAsync(SelectedTermId);
                    if (term != null)
                    {
                        TermStartDateIso = term.StartDate.ToString("yyyy-MM-dd");
                    }
                    CurrentEnrollments = await _enrollmentService.GetStudentEnrollmentsByTermAsync(StudentCode, SelectedTermId);
                }
            }

            return Page();
        }

        // Endpoint for fetching available sections of a chosen course
        public async Task<IActionResult> OnGetAvailableSectionsAsync(Guid courseId, Guid termId)
        {
            if (courseId == Guid.Empty || termId == Guid.Empty)
                return new JsonResult(new List<SectionDTO>());

            var sections = await _enrollmentService.GetAvailableSectionsForCourseAsync(courseId, termId);
            return new JsonResult(sections);
        }

        // Endpoint for Student Timetable (FullCalendar)
        public async Task<IActionResult> OnGetStudentTimetableAsync(string studentCode, Guid termId)
        {
            if (string.IsNullOrEmpty(studentCode) || termId == Guid.Empty)
                return new JsonResult(new List<StudentScheduleDTO>());

            var events = await _enrollmentService.GetStudentTimetableAsync(studentCode, termId);
            return new JsonResult(events);
        }

        public async Task<IActionResult> OnPostRegisterAsync(Guid sectionId)
        {
            var studentCodeClaim = User.FindFirst("StudentCode")?.Value;
            
            // Re-fetch StudentCode if claims are wiped (or just AccountId exists)
            if (string.IsNullOrEmpty(studentCodeClaim))
            {
                var accountIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(accountIdStr) && Guid.TryParse(accountIdStr, out var accountId))
                {
                    // Usually we'd inject DbContext to fetch auth state, but since EnrollmentService depends on DB we could technically inject DB Context here.
                    // For now let's hope the claim logic fixed earlier is robust, otherwise we return error.
                    return new JsonResult(new { success = false, message = "Session expired or invalid student profile. Please logout and login again." });
                }
            }

            if (string.IsNullOrEmpty(studentCodeClaim) || sectionId == Guid.Empty)
            {
                return new JsonResult(new { success = false, message = "Invalid request parameters." });
            }

            var result = await _enrollmentService.RegisterForSectionAsync(studentCodeClaim, sectionId);
            return new JsonResult(new { success = result.IsSuccess, message = result.ErrorMessage });
        }

        public async Task<IActionResult> OnPostDropAsync([FromBody] DropRequest request)
        {
            var sectionId = request.SectionId;
            var studentCodeClaim = User.FindFirst("StudentCode")?.Value;
            
            if (string.IsNullOrEmpty(studentCodeClaim) || sectionId == Guid.Empty)
            {
                return new JsonResult(new { success = false, message = "Invalid request parameters." });
            }

            var success = await _enrollmentService.RemoveRegistrationAsync(studentCodeClaim, sectionId);
            if (success)
            {
                return new JsonResult(new { success = true, message = "Successfully unregistered from the class." });
            }
            
            return new JsonResult(new { success = false, message = "Failed to unregister. You may not be enrolled in this class." });
        }
    }

    public class DropRequest
    {
        public Guid SectionId { get; set; }
    }
}
