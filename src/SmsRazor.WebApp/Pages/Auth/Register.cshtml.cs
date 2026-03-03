using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IAccountService _accountService;

    public RegisterModel(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [BindProperty]
    public RegisterInputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.Password != Input.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            return Page();
        }

        // Check if username/email already exists via BLL
        bool emailExists = await _accountService.IsEmailRegisteredAsync(Input.Email);
        if (emailExists)
        {
            ModelState.AddModelError(string.Empty, "Email is already registered.");
            return Page();
        }

        // Register via BLL
        await _accountService.RegisterAdminAsync(Input.Email, Input.Password, Input.FullName);

        return RedirectToPage("/Auth/Login");
    }
}

public class RegisterInputModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool AgreeToTerms { get; set; }
}
