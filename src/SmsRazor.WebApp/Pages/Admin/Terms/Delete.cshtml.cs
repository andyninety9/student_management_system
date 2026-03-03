using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Terms
{
    [Authorize(Roles = "Admin,Root")]
    public class DeleteModel : PageModel
    {
        private readonly ITermService _termService;

        public DeleteModel(ITermService termService)
        {
            _termService = termService;
        }

        [BindProperty]
        public TermDTO Term { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var term = await _termService.GetTermByIdAsync(id);
            if (term == null)
            {
                return NotFound();
            }

            Term = term;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var success = await _termService.DeleteTermAsync(Term.TermId);
                if (success)
                {
                    StatusMessage = $"Success: Semester '{Term.Name}' was deleted successfully.";
                }
                else
                {
                    StatusMessage = "Error: Failed to delete the semester.";
                }

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }
    }
}
