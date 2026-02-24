using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class IndexModel : PageModel
    {
        private readonly IAttendanceService _attendanceService;

        public IndexModel(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        public IEnumerable<UpcomingClassDTO> UpcomingClasses { get; set; } = new List<UpcomingClassDTO>();

        public async Task OnGetAsync()
        {
            var studentCodeClaim = User.FindFirst("StudentCode")?.Value;
            if (!string.IsNullOrEmpty(studentCodeClaim))
            {
                UpcomingClasses = await _attendanceService.GetStudentUpcomingClassesAsync(studentCodeClaim, 3);
            }
        }
    }
}
