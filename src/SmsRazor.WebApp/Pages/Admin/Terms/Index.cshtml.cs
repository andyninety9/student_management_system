using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Terms
{
    [Authorize(Roles = "Admin,Root")]
    public class IndexModel : PageModel
    {
        private readonly ITermService _termService;

        public IndexModel(ITermService termService)
        {
            _termService = termService;
        }

        public IEnumerable<TermDTO> Terms { get; set; } = new List<TermDTO>();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Terms = await _termService.GetAllTermsAsync();
            return Page();
        }
    }
}
