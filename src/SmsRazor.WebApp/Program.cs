using Microsoft.EntityFrameworkCore;

// Load .env file if it exists (for runtime)
if (File.Exists(".env"))
{
    Console.WriteLine("✓ .env file found");
    DotNetEnv.Env.Load();
}
else
{
    Console.WriteLine("✗ .env file NOT found");
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddAuthentication("JwtCookie")
    .AddJwtBearer("JwtCookie", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? "this_is_a_fallback_secret_that_should_not_be_used")),
            ValidateIssuer = false, // Simplify for this example
            ValidateAudience = false, // Simplify for this example
            ClockSkew = TimeSpan.Zero
        };
    });

// Build connection string from .env variables (if available), otherwise use appsettings.json
var connectionString = Environment.GetEnvironmentVariable("DB_HOST") != null
    ? $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Port={Environment.GetEnvironmentVariable("DB_PORT")};Database={Environment.GetEnvironmentVariable("DB_NAME")};Username={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASS")}"
    : builder.Configuration.GetConnectionString("DefaultConnection");

// Debug output to verify configuration
Console.WriteLine("\n=== Database Configuration ===");
Console.WriteLine($"DB_HOST env var: {Environment.GetEnvironmentVariable("DB_HOST") ?? "NULL"}");
Console.WriteLine($"DB_PORT env var: {Environment.GetEnvironmentVariable("DB_PORT") ?? "NULL"}");
Console.WriteLine($"DB_NAME env var: {Environment.GetEnvironmentVariable("DB_NAME") ?? "NULL"}");
Console.WriteLine($"DB_USER env var: {Environment.GetEnvironmentVariable("DB_USER") ?? "NULL"}");
Console.WriteLine($"DB_PASS env var: {(Environment.GetEnvironmentVariable("DB_PASS") != null ? "***SET***" : "NULL")}");
Console.WriteLine($"\nConnection String Source: {(Environment.GetEnvironmentVariable("DB_HOST") != null ? ".env file" : "appsettings.json")}");
Console.WriteLine($"Connection String: {connectionString.Replace(Environment.GetEnvironmentVariable("DB_PASS") ?? "", "***")}");
Console.WriteLine("==============================\n");

builder.Services.AddDbContext<SmsRazor.DAL.Data.SmsDbContext>(options =>
    options.UseNpgsql(connectionString));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/NotFound");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Ensure static files are served before auth check

app.UseRouting();

// Custom JWT Cookie Middleware
app.UseMiddleware<SmsRazor.WebApp.Middleware.JwtCookieMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
