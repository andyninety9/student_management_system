using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Student.Attendance
{
    [Authorize(Roles = "Student")]
    public class IndexModel : PageModel
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ISectionService _sectionService;

        public IndexModel(IAttendanceService attendanceService, ISectionService sectionService)
        {
            _attendanceService = attendanceService;
            _sectionService = sectionService;
        }

        [BindProperty(SupportsGet = true)]
        public Guid SelectedTermId { get; set; }

        public SelectList TermsList { get; set; } = default!;
        
        public IEnumerable<AttendanceRecordDTO> AttendanceRecords { get; set; } = new List<AttendanceRecordDTO>();

        public async Task<IActionResult> OnGetAsync()
        {
            var termsLookup = await _sectionService.GetTermsLookupAsync();
            TermsList = new SelectList(termsLookup, "Key", "Value");

            var studentCodeClaim = User.FindFirst("StudentCode")?.Value;
            
            if (SelectedTermId != Guid.Empty && !string.IsNullOrEmpty(studentCodeClaim))
            {
                AttendanceRecords = await _attendanceService.GetStudentAttendanceRecordsAsync(studentCodeClaim, SelectedTermId);
            }

            return Page();
        }
    }
}
