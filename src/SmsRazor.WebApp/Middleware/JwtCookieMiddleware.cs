using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SmsRazor.WebApp.Middleware;

public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtCookieMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Cookies["accessToken"];
        var path = context.Request.Path.Value?.ToLower();

        // 1. Allow public paths (Auth pages, static assets, error pages)
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // 2. If no token, redirect to login
        if (string.IsNullOrEmpty(token))
        {
            RedirectToLogin(context);
            return;
        }

        // 3. Validate token
        if (!AttachUserToContext(context, token))
        {
            // Token invalid or expired
            context.Response.Cookies.Delete("accessToken");
            RedirectToLogin(context);
            return;
        }

        await _next(context);
    }

    private bool IsPublicPath(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        return path.StartsWith("/auth") || 
               path.StartsWith("/css") || 
               path.StartsWith("/js") || 
               path.StartsWith("/lib") || 
               path.StartsWith("/images") ||
               path.StartsWith("/student/tuition/callback") ||
               path.Equals("/notfound") ||
               path.Equals("/error");
    }

    private void RedirectToLogin(HttpContext context)
    {
        // Don't redirect if it's an API call or AJAX request (optional refinement)
        var returnUrl = context.Request.Path + context.Request.QueryString;
        context.Response.Redirect($"/Auth/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
    }

    private bool AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var secret = _configuration["JWT_SECRET"];
            if (string.IsNullOrEmpty(secret)) return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "SmsRazor",
                ValidateAudience = true,
                ValidAudience = "SmsRazorUsers",
                // Set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 mins later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            // Extract mapped claims from the principal (this preserves ClaimTypes like ClaimTypes.Role)
            var claims = principal.Claims.ToList();

            // Create identity and principal, explicitly stating the Name and Role claim types 
            // so [Authorize(Roles = "...")] works perfectly.
            var identity = new ClaimsIdentity(claims, "JwtCookie", ClaimTypes.Name, ClaimTypes.Role);
            context.User = new ClaimsPrincipal(identity);

            return true;
        }
        catch
        {
            // Token validation failed
            return false;
        }
    }
}

// Extension method for easy registration
public static class JwtCookieMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtCookieMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtCookieMiddleware>();
    }
}
