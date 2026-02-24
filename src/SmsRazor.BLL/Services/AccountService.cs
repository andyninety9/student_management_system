using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsRazor.BLL.DTOs;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

namespace SmsRazor.BLL.Services;

public class AccountService : IAccountService
{
    private readonly SmsDbContext _context;

    public AccountService(SmsDbContext context)
    {
        _context = context;
    }

    public async Task InitializeSystemAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch
        {
            // Logging or handling for migration exception
        }

        bool hasAnyAccount = await _context.Accounts.AnyAsync();
        if (!hasAnyAccount)
        {
            // Ensure Root Role
            var rootRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Root");
            if (rootRole == null)
            {
                rootRole = new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "Root",
                    IsActive = true
                };
                _context.Roles.Add(rootRole);
            }

            // Ensure Admin Role
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "Admin",
                    IsActive = true
                };
                _context.Roles.Add(adminRole);
            }

            // Create Root Account
            var rootAccount = new Account
            {
                AccountId = Guid.NewGuid(),
                Username = "root",
                Email = "sms-account@fpt.edu.vn",
                PasswordHash = HashPassword("Sms@123456"),
                Fullname = "System Administrator",
                RoleId = rootRole.RoleId,
                IsActive = true,
                EmailVerified = true
            };
            _context.Accounts.Add(rootAccount);
            
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsEmailRegisteredAsync(string email)
    {
        return await _context.Accounts.AnyAsync(a => a.Username == email || a.Email == email);
    }

    public async Task RegisterAdminAsync(string email, string password, string fullName)
    {
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = "Admin",
                IsActive = true
            };
            _context.Roles.Add(adminRole);
            await _context.SaveChangesAsync();
        }

        var newAdmin = new Account
        {
            AccountId = Guid.NewGuid(),
            // Using Email as Username since there is no separate username field in the UI
            Username = email, 
            Email = email,
            PasswordHash = HashPassword(password),
            Fullname = fullName,
            RoleId = adminRole.RoleId,
            IsActive = false, // DISABLED BY DEFAULT, waiting for root to enable manually
            EmailVerified = false
        };

        _context.Accounts.Add(newAdmin);
        await _context.SaveChangesAsync();
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var account = await _context.Accounts
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.Username == email || a.Email == email);

        if (account == null)
            return new LoginResult { IsSuccess = false, ErrorMessage = "Invalid email or password." };

        if (!account.IsActive)
            return new LoginResult { IsSuccess = false, ErrorMessage = "Account is disabled. Please contact the administrator." };

        var hashedInputPassword = HashPassword(password);
        if (account.PasswordHash != hashedInputPassword)
            return new LoginResult { IsSuccess = false, ErrorMessage = "Invalid email or password." };

        var result = new LoginResult
        {
            IsSuccess = true,
            AccountId = account.AccountId,
            Email = account.Email ?? account.Username,
            FullName = account.Fullname ?? string.Empty,
            RoleName = account.Role?.RoleName ?? "User"
        };
        
        if (result.RoleName == "Student")
        {
            var studentInfo = await _context.StudentInfos.FirstOrDefaultAsync(s => s.AccountId == account.AccountId);
            if (studentInfo != null)
            {
                result.StudentCode = studentInfo.StudentCode;
            }
        }

        return result;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
