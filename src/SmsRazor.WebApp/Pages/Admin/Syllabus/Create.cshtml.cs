using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Syllabus
{
    [Authorize(Roles = "Admin,Root")]
    public class CreateModel : PageModel
    {
        private readonly ISyllabusService _syllabusService;

        public CreateModel(ISyllabusService syllabusService)
        {
            _syllabusService = syllabusService;
        }

        [BindProperty]
        public SyllabusDTO Syllabus { get; set; } = new SyllabusDTO()
        {
            EffectiveFrom = DateTime.Today,
            IsActive = true,
            Status = "Active"
        };

        public SelectList DepartmentsList { get; set; } = default!;
        public MultiSelectList CoursesList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            var departments = await _syllabusService.GetDepartmentsLookupAsync();
            DepartmentsList = new SelectList(departments, "Key", "Value");
            
            var courses = await _syllabusService.GetCoursesLookupAsync();
            CoursesList = new MultiSelectList(courses, "Key", "Value");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var departments = await _syllabusService.GetDepartmentsLookupAsync();
                DepartmentsList = new SelectList(departments, "Key", "Value");
                
                var courses = await _syllabusService.GetCoursesLookupAsync();
                CoursesList = new MultiSelectList(courses, "Key", "Value");
                
                return Page();
            }

            await _syllabusService.CreateSyllabusAsync(Syllabus);
            return RedirectToPage("./Index");
        }
    }
}
