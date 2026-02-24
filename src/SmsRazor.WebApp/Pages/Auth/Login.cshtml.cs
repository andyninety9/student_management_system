using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAccountService _accountService;
    private readonly IConfiguration _configuration;

    public LoginModel(IAccountService accountService, IConfiguration configuration)
    {
        _accountService = accountService;
        _configuration = configuration;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _accountService.LoginAsync(Input.Email, Input.Password);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return Page();
        }

        var token = GenerateJwtToken(result);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = Input.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(2)
        };

        Response.Cookies.Append("accessToken", token, cookieOptions);

        // Role-based redirection
        if (result.RoleName == "Admin" || result.RoleName == "Root")
        {
            return RedirectToPage("/Admin/Index");
        }
        else if (result.RoleName == "Student")
        {
            return RedirectToPage("/Student/Index");
        }

        return LocalRedirect(ReturnUrl ?? "/");
    }

    private string GenerateJwtToken(SmsRazor.BLL.DTOs.LoginResult accountInfo)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secret = _configuration["JWT_SECRET"] ?? "this_is_a_fallback_secret_that_should_not_be_used";
        var key = Encoding.ASCII.GetBytes(secret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, accountInfo.AccountId.ToString()),
            new Claim(ClaimTypes.Email, accountInfo.Email),
            new Claim(ClaimTypes.Name, accountInfo.FullName),
            new Claim(ClaimTypes.Role, accountInfo.RoleName)
        };

        if (accountInfo.RoleName == "Student" && !string.IsNullOrEmpty(accountInfo.StudentCode))
        {
            claims.Add(new Claim("StudentCode", accountInfo.StudentCode));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = Input.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class LoginInputModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
