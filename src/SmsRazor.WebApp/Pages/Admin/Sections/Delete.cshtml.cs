using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Sections
{
    [Authorize(Roles = "Admin,Root")]
    public class DeleteModel : PageModel
    {
        private readonly ISectionService _sectionService;

        public DeleteModel(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [BindProperty]
        public SectionDTO SectionDto { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var section = await _sectionService.GetSectionByIdAsync(id);
            if (section == null)
            {
                return NotFound();
            }

            SectionDto = section;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var success = await _sectionService.DeleteSectionAsync(SectionDto.SectionId);
            if (success)
            {
                StatusMessage = "Success: Class and its timetables were completely deleted.";
            }
            else
            {
                StatusMessage = "Error: Failed to delete the class.";
            }

            return RedirectToPage("./Index");
        }
    }
}
