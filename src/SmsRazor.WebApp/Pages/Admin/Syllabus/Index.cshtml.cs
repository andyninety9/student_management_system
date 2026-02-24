using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Syllabus
{
    [Authorize(Roles = "Admin,Root")]
    public class IndexModel : PageModel
    {
        private readonly ISyllabusService _syllabusService;

        public IndexModel(ISyllabusService syllabusService)
        {
            _syllabusService = syllabusService;
        }

        public IEnumerable<SyllabusDTO> Syllabi { get; set; } = new List<SyllabusDTO>();

        public async Task OnGetAsync()
        {
            Syllabi = await _syllabusService.GetAllSyllabiAsync();
        }
    }
}
