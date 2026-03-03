using System.ComponentModel.DataAnnotations;
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
    public class CreateModel : PageModel
    {
        private readonly ITeacherService _teacherService;

        public CreateModel(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [BindProperty]
        public TeacherDTO Teacher { get; set; } = new TeacherDTO();

        public SelectList Departments { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
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
                await _teacherService.CreateTeacherAsync(Teacher);
                StatusMessage = $"Success: Teacher '{Teacher.TeacherCode}' created successfully.";
                return RedirectToPage("./Index");
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
