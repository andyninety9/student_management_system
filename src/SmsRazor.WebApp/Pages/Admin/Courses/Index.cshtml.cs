using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Courses
{
    [Authorize(Roles = "Admin,Root")]
    public class IndexModel : PageModel
    {
        private readonly ICourseService _courseService;

        public IndexModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public IEnumerable<CourseDTO> Courses { get; set; } = new List<CourseDTO>();

        public async Task OnGetAsync()
        {
            Courses = await _courseService.GetAllCoursesAsync();
        }
    }
}
