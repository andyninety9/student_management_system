using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Sections
{
    [Authorize(Roles = "Admin,Root")]
    public class IndexModel : PageModel
    {
        private readonly ISectionService _sectionService;

        public IndexModel(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        public IEnumerable<SectionDTO> Sections { get; set; } = new List<SectionDTO>();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Sections = await _sectionService.GetAllSectionsAsync();
            return Page();
        }
    }
}
