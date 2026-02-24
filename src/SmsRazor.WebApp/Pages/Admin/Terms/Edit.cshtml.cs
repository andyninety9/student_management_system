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
    public class EditModel : PageModel
    {
        private readonly ITermService _termService;

        public EditModel(ITermService termService)
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

            // Fix time binding issues for HTML5 date inputs by dropping time components.
            term.StartDate = term.StartDate.Date;
            term.EndDate = term.EndDate.Date;
            
            Term = term;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var success = await _termService.UpdateTermAsync(Term);
                if (success)
                {
                    StatusMessage = $"Success: Term '{Term.Name}' updated successfully.";
                    return RedirectToPage("./Index");
                }

                ModelState.AddModelError(string.Empty, "Update failed. Term might have been deleted.");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}
