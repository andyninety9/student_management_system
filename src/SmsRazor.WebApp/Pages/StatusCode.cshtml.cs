using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmsRazor.WebApp.Pages
{
    public class StatusCodeModel : PageModel
    {
        public IActionResult OnGet(int code)
        {
            if (code == 404)
            {
                return RedirectToPage("/NotFound");
            }
            else if (code == 401 || code == 403)
            {
                return RedirectToPage("/AccessDenied");
            }
            
            // Fallback for other errors (500 etc)
            return RedirectToPage("/Error");
        }
    }
}
