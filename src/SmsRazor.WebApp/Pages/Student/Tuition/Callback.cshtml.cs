using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Student.Tuition;

[Microsoft.AspNetCore.Authorization.AllowAnonymous]
[Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken(Order = 1001)]
public class CallbackModel : PageModel
{
    private readonly ITuitionService _tuitionService;
    private readonly IConfiguration _config;

    public CallbackModel(ITuitionService tuitionService, IConfiguration config)
    {
        _tuitionService = tuitionService;
        _config = config;
    }

    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (Request.Query.Count == 0)
        {
            return RedirectToPage("/Student/Tuition/Index");
        }

        IsSuccess = await _tuitionService.ProcessVnPayCallbackAsync(Request.Query, _config);

        return Page();
    }
}
