using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Teachers
{
    [Authorize(Roles = "Admin,Root")]
    public class EditModel : PageModel
    {
        private readonly ITeacherService _teacherService;

        public EditModel(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [BindProperty]
        public TeacherDTO Teacher { get; set; } = default!;

        public SelectList Departments { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var teacher = await _teacherService.GetTeacherByCodeAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            Teacher = teacher;
            await PopulateDropDownsAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync();
                return Page();
            }

            try
            {
                var success = await _teacherService.UpdateTeacherAsync(Teacher);
                if (success)
                {
                    StatusMessage = $"Success: Teacher '{Teacher.TeacherCode}' updated successfully.";
                    return RedirectToPage("./Index");
                }
                
                ModelState.AddModelError(string.Empty, "Update failed. The teacher might have been deleted.");
                await PopulateDropDownsAsync();
                return Page();
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropDownsAsync();
                return Page();
            }
        }

        private async Task PopulateDropDownsAsync()
        {
            var depts = await _teacherService.GetDepartmentsLookupAsync();
            Departments = new SelectList(depts, "Key", "Value");
        }
    }
}
