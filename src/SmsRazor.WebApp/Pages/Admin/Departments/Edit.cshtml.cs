using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Departments;

public class EditModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    public EditModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [BindProperty]
    public DepartmentDTO Department { get; set; } = new DepartmentDTO();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (id == null) return NotFound();

        var department = await _departmentService.GetDepartmentByIdAsync(id.Value);
        if (department == null) return NotFound();

        Department = department;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _departmentService.UpdateDepartmentAsync(Department);
        if (!result) return NotFound();

        return RedirectToPage("./Index");
    }
}
