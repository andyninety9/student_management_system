using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsRazor.BLL.DTOs;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

namespace SmsRazor.BLL.Services;

public class TeacherService : ITeacherService
{
    private readonly SmsDbContext _context;

    public TeacherService(SmsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TeacherDTO>> GetAllTeachersAsync()
    {
        var teachers = await _context.TeacherInfos
            .Include(t => t.Account)
            .Include(t => t.Department)
            .OrderByDescending(t => t.Account!.CreatedAt)
            .ToListAsync();

        return teachers.Select(MapToDTO);
    }

    public async Task<TeacherDTO?> GetTeacherByCodeAsync(string teacherCode)
    {
        var teacher = await _context.TeacherInfos
            .Include(t => t.Account)
            .Include(t => t.Department)
            .FirstOrDefaultAsync(t => t.TeacherCode == teacherCode);

        if (teacher == null) return null;
        return MapToDTO(teacher);
    }

    public async Task<TeacherDTO?> GetTeacherByIdAsync(Guid accountId)
    {
        var teacher = await _context.TeacherInfos
            .Include(t => t.Account)
            .Include(t => t.Department)
            .FirstOrDefaultAsync(t => t.AccountId == accountId);

        if (teacher == null) return null;
        return MapToDTO(teacher);
    }

    public async Task<string?> CreateTeacherAsync(TeacherDTO dto)
    {
        // 1. Validate uniqueness
        if (await _context.Accounts.AnyAsync(a => a.Email == dto.Email || a.Username == dto.Email))
            throw new Exception("Email is already registered.");

        if (await _context.TeacherInfos.AnyAsync(t => t.TeacherCode == dto.TeacherCode))
            throw new Exception("Teacher Code already exists.");

        // 2. Ensure Teacher Role exists
        var teacherRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Teacher");
        if (teacherRole == null)
        {
            teacherRole = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = "Teacher",
                IsActive = true
            };
            _context.Roles.Add(teacherRole);
            await _context.SaveChangesAsync();
        }

        // 3. Create Account
        var accountId = Guid.NewGuid();
        var account = new Account
        {
            AccountId = accountId,
            Username = dto.Email,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password ?? "123456"),
            Fullname = dto.Fullname,
            Gender = dto.Gender,
            Dob = dto.Dob.HasValue ? DateTime.SpecifyKind(dto.Dob.Value, DateTimeKind.Utc) : null,
            Phone = dto.Phone,
            Address = dto.Address,
            RoleId = teacherRole.RoleId,
            IsActive = dto.IsActive,
            EmailVerified = true // Auto-verify for admin created
        };

        _context.Accounts.Add(account);

        // 4. Create TeacherInfo
        var teacherInfo = new TeacherInfo
        {
            TeacherCode = dto.TeacherCode,
            AccountId = accountId,
            DepartmentId = dto.DepartmentId,
            Title = dto.Title
        };

        _context.TeacherInfos.Add(teacherInfo);
        await _context.SaveChangesAsync();

        return teacherInfo.TeacherCode;
    }

    public async Task<bool> UpdateTeacherAsync(TeacherDTO dto)
    {
        var teacherInfo = await _context.TeacherInfos
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.TeacherCode == dto.TeacherCode);

        if (teacherInfo == null || teacherInfo.Account == null) return false;

        // Check email uniqueness if email changed
        if (teacherInfo.Account.Email != dto.Email)
        {
            if (await _context.Accounts.AnyAsync(a => a.AccountId != teacherInfo.AccountId && (a.Email == dto.Email || a.Username == dto.Email)))
            {
                throw new Exception("Email is already used by another account.");
            }
        }

        // Update Account
        teacherInfo.Account.Email = dto.Email;
        teacherInfo.Account.Username = dto.Email;
        teacherInfo.Account.Fullname = dto.Fullname;
        teacherInfo.Account.Gender = dto.Gender;
        teacherInfo.Account.Dob = dto.Dob.HasValue ? DateTime.SpecifyKind(dto.Dob.Value, DateTimeKind.Utc) : null;
        teacherInfo.Account.Phone = dto.Phone;
        teacherInfo.Account.Address = dto.Address;
        teacherInfo.Account.IsActive = dto.IsActive;

        // Update TeacherInfo
        teacherInfo.DepartmentId = dto.DepartmentId;
        teacherInfo.Title = dto.Title;

        _context.Accounts.Update(teacherInfo.Account);
        _context.TeacherInfos.Update(teacherInfo);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteTeacherAsync(string teacherCode)
    {
        var teacherInfo = await _context.TeacherInfos.FirstOrDefaultAsync(t => t.TeacherCode == teacherCode);
        if (teacherInfo == null) return false;

        var account = await _context.Accounts.FindAsync(teacherInfo.AccountId);
        
        _context.TeacherInfos.Remove(teacherInfo);
        if (account != null)
        {
            _context.Accounts.Remove(account);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetTeacherPasswordAsync(string teacherCode, string newPassword)
    {
        var teacherInfo = await _context.TeacherInfos
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.TeacherCode == teacherCode);

        if (teacherInfo == null || teacherInfo.Account == null) return false;

        teacherInfo.Account.PasswordHash = HashPassword(newPassword);
        _context.Accounts.Update(teacherInfo.Account);
        await _context.SaveChangesAsync();

        return true;
    }

    // Lookups
    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetDepartmentsLookupAsync()
    {
        var list = await _context.Departments.Where(x => x.IsActive).OrderBy(x => x.DepartmentNameEng).ToListAsync();
        return list.Select(x => new KeyValuePair<Guid, string>(x.DepartmentId, x.DepartmentNameEng));
    }

    private TeacherDTO MapToDTO(TeacherInfo t)
    {
        return new TeacherDTO
        {
            AccountId = t.AccountId,
            TeacherCode = t.TeacherCode,
            Email = t.Account?.Email ?? string.Empty,
            Fullname = t.Account?.Fullname ?? string.Empty,
            Gender = t.Account?.Gender,
            Phone = t.Account?.Phone,
            Dob = t.Account?.Dob,
            Address = t.Account?.Address,
            IsActive = t.Account?.IsActive ?? false,
            DepartmentId = t.DepartmentId,
            Title = t.Title,
            DepartmentName = t.Department?.DepartmentNameEng
        };
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
