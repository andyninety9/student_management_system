using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmsRazor.WebApp.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            // JWT sometimes maps ClaimTypes.Role, and sometimes leaves it as "role"
            var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
            
            if (role == "Admin" || role == "Root")
            {
                return RedirectToPage("/Admin/Index");
            }
            else if (role == "Student")
            {
                return RedirectToPage("/Student/Index");
            }
            else if (role == "Teacher")
            {
                // Placeholder redirect for teachers
                return RedirectToPage("/Teacher/Index");
            }
            
            // If they have an unknown role, they shouldn't see the blank page, 
            // kick them to AccessDenied as a safe fallback
            return RedirectToPage("/AccessDenied");
        }

        // Technically, JwtCookieMiddleware should catch unauthenticated users before they reach here,
        // but just in case:
        return RedirectToPage("/Auth/Login");
    }
}
