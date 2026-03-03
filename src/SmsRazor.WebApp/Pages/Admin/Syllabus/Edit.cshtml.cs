using System;
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
    public class EditModel : PageModel
    {
        private readonly ISyllabusService _syllabusService;

        public EditModel(ISyllabusService syllabusService)
        {
            _syllabusService = syllabusService;
        }

        [BindProperty]
        public SyllabusDTO Syllabus { get; set; } = new SyllabusDTO();

        public SelectList DepartmentsList { get; set; } = default!;
        public MultiSelectList CoursesList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null) return NotFound();

            var syllabus = await _syllabusService.GetSyllabusByIdAsync(id.Value);
            if (syllabus == null) return NotFound();

            Syllabus = syllabus;
            
            var departments = await _syllabusService.GetDepartmentsLookupAsync();
            DepartmentsList = new SelectList(departments, "Key", "Value");
            
            var courses = await _syllabusService.GetCoursesLookupAsync();
            CoursesList = new MultiSelectList(courses, "Key", "Value");

            return Page();
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

            var success = await _syllabusService.UpdateSyllabusAsync(Syllabus);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToPage("./Index");
        }
    }
}
