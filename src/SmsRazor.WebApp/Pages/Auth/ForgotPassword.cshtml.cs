using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmsRazor.WebApp.Pages.Auth;

public class ForgotPasswordModel : PageModel
{
    [BindProperty]
    public ForgotPasswordInputModel Input { get; set; } = new();

    public bool EmailSent { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO: Implement password reset logic
        // For now, just show success message
        EmailSent = true;
        return Page();
    }
}

public class ForgotPasswordInputModel
{
    public string Email { get; set; } = string.Empty;
}
