using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Admin.Departments;

[Authorize(Roles = "Admin,Root")]
public class IndexModel : PageModel
{
    private readonly IDepartmentService _departmentService;

    public IndexModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    public IEnumerable<DepartmentDTO> Departments { get; set; } = new List<DepartmentDTO>();

    public async Task<IActionResult> OnGetAsync()
    {
        Departments = await _departmentService.GetAllDepartmentsAsync();
        return Page();
    }
}
