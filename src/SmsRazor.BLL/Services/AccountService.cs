using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsRazor.BLL.DTOs;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

using SmsRazor.DAL.Repositories;

namespace SmsRazor.BLL.Services;

public class AccountService : IAccountService
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<StudentInfo> _studentInfoRepository;
    private readonly SmsDbContext _context;

    public AccountService(
        IRepository<Account> accountRepository,
        IRepository<Role> roleRepository,
        IRepository<StudentInfo> studentInfoRepository,
        SmsDbContext context)
    {
        _accountRepository = accountRepository;
        _roleRepository = roleRepository;
        _studentInfoRepository = studentInfoRepository;
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

        bool hasAnyAccount = await _accountRepository.Entities.AnyAsync();
        if (!hasAnyAccount)
        {
            // Ensure Root Role
            var rootRole = await _roleRepository.Entities.FirstOrDefaultAsync(r => r.RoleName == "Root");
            if (rootRole == null)
            {
                rootRole = new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "Root",
                    IsActive = true
                };
                await _roleRepository.AddAsync(rootRole);
            }

            // Ensure Admin Role
            var adminRole = await _roleRepository.Entities.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "Admin",
                    IsActive = true
                };
                await _roleRepository.AddAsync(adminRole);
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
            await _accountRepository.AddAsync(rootAccount);
            
            await _accountRepository.SaveChangesAsync();
        }
    }

    public async Task<bool> IsEmailRegisteredAsync(string email)
    {
        return await _accountRepository.Entities.AnyAsync(a => a.Username == email || a.Email == email);
    }

    public async Task RegisterAdminAsync(string email, string password, string fullName)
    {
        var adminRole = await _roleRepository.Entities.FirstOrDefaultAsync(r => r.RoleName == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = "Admin",
                IsActive = true
            };
            await _roleRepository.AddAsync(adminRole);
            await _roleRepository.SaveChangesAsync();
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

        await _accountRepository.AddAsync(newAdmin);
        await _accountRepository.SaveChangesAsync();
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var account = await _accountRepository.Entities
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
            var studentInfo = await _studentInfoRepository.Entities.FirstOrDefaultAsync(s => s.AccountId == account.AccountId);
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
