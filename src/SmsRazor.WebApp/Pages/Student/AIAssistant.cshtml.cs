using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmsRazor.WebApp.Pages.Student;

[Authorize(Roles = "Student")]
public class AIAssistantModel : PageModel
{
    public void OnGet()
    {
    }
}
