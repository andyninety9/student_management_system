using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Terms
{
    [Authorize(Roles = "Admin,Root")]
    public class CreateModel : PageModel
    {
        private readonly ITermService _termService;

        public CreateModel(ITermService termService)
        {
            _termService = termService;
        }

        [BindProperty]
        public TermDTO Term { get; set; } = new TermDTO
        {
            StartDate = System.DateTime.Today,
            EndDate = System.DateTime.Today.AddMonths(3)
        };

        [TempData]
        public string? StatusMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _termService.CreateTermAsync(Term);
                StatusMessage = $"Success: Term '{Term.Name}' created successfully.";
                return RedirectToPage("./Index");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}
