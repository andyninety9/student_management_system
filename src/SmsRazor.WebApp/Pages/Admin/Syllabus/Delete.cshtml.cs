using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Syllabus
{
    [Authorize(Roles = "Admin,Root")]
    public class DeleteModel : PageModel
    {
        private readonly ISyllabusService _syllabusService;

        public DeleteModel(ISyllabusService syllabusService)
        {
            _syllabusService = syllabusService;
        }

        [BindProperty]
        public SyllabusDTO Syllabus { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var syllabus = await _syllabusService.GetSyllabusByIdAsync(id.Value);

            if (syllabus == null)
            {
                return NotFound();
            }

            Syllabus = syllabus;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var success = await _syllabusService.DeleteSyllabusAsync(id.Value);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToPage("./Index");
        }
    }
}
