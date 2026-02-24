using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Students
{
    [Authorize(Roles = "Admin,Root")]
    public class CreateModel : PageModel
    {
        private readonly IStudentService _studentService;

        public CreateModel(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [BindProperty]
        public StudentDTO Student { get; set; } = new StudentDTO();

        public SelectList Departments { get; set; } = default!;
        public SelectList Syllabuses { get; set; } = default!;
        public SelectList Intakes { get; set; } = default!;
        public SelectList Statuses { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync();
                return Page();
            }

            try
            {
                await _studentService.CreateStudentAsync(Student);
                TempData["StatusMessage"] = $"Success: Student {Student.StudentCode} created successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadLookupsAsync();
                return Page();
            }
        }

        private async Task LoadLookupsAsync()
        {
            Departments = new SelectList(await _studentService.GetDepartmentsLookupAsync(), "Key", "Value");
            Syllabuses = new SelectList(await _studentService.GetSyllabusesLookupAsync(), "Key", "Value");
            Intakes = new SelectList(await _studentService.GetIntakesLookupAsync(), "Key", "Value");
            Statuses = new SelectList(await _studentService.GetStudentStatusesLookupAsync(), "Key", "Value");
        }
    }
}
