using Microsoft.EntityFrameworkCore;
using SmsRazor.BLL;

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

// Increase Kestrel Max Request Body Size for 500MB uploads
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524_288_000;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Increase Multipart Body Length for 500MB file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524_288_000;
});
builder.Services.AddAuthentication("JwtCookie")
    .AddJwtBearer("JwtCookie", options =>
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("JWT_SECRET environment variable is not configured.");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = "SmsRazor",
            ValidateAudience = true,
            ValidAudience = "SmsRazorUsers",
            ClockSkew = TimeSpan.Zero
        };
    });

// Build connection string from .env variables (if available), otherwise use appsettings.json
var connectionString = Environment.GetEnvironmentVariable("DB_HOST") != null
    ? $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Port={Environment.GetEnvironmentVariable("DB_PORT")};Database={Environment.GetEnvironmentVariable("DB_NAME")};Username={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASS")}"
    : builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

// Register BLL and DAL services through BLL's extension method
builder.Services.AddBusinessLayer(connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // Handle specific status codes safely mapping 404 and 403.
    // {0} is automatically replaced with the status code like 404 or 403
    app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Ensure static files are served before auth check

app.UseRouting();

// Custom JWT Cookie Middleware
app.UseMiddleware<SmsRazor.WebApp.Middleware.JwtCookieMiddleware>();

app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.MapHub<SmsRazor.BLL.Hubs.ChatHub>("/chatHub");
app.MapHub<SmsRazor.BLL.Hubs.AssistantHub>("/assistantHub");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var accountService = services.GetRequiredService<SmsRazor.BLL.Services.IAccountService>();
    await accountService.InitializeSystemAsync();
}

app.Run();
