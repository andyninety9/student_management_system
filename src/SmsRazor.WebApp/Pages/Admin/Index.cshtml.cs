using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmsRazor.WebApp.Pages.Admin;

[Authorize(Roles = "Admin,Root")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
        // Add data loading logic here in the future
    }
}
