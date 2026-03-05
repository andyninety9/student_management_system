using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Departments;

[Authorize(Roles = "Admin,Root")]
public class DeleteModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    public DeleteModel(IDepartmentService departmentService)
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

    public async Task<IActionResult> OnPostAsync(Guid? id)
    {
        if (id == null) return NotFound();

        var result = await _departmentService.DeleteDepartmentAsync(id.Value);
        if (!result) return NotFound();

        return RedirectToPage("./Index");
    }
}
