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

public class StudentService : IStudentService
{
    private readonly SmsDbContext _context;

    public StudentService(SmsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StudentDTO>> GetAllStudentsAsync()
    {
        var students = await _context.StudentInfos
            .Include(s => s.Account)
            .Include(s => s.Department)
            .Include(s => s.Syllabus)
            .Include(s => s.Intake)
            .Include(s => s.StudentStatus)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return students.Select(MapToDTO);
    }

    public async Task<StudentDTO?> GetStudentByCodeAsync(string studentCode)
    {
        var student = await _context.StudentInfos
            .Include(s => s.Account)
            .Include(s => s.Department)
            .Include(s => s.Syllabus)
            .Include(s => s.Intake)
            .Include(s => s.StudentStatus)
            .FirstOrDefaultAsync(s => s.StudentCode == studentCode);

        if (student == null) return null;
        return MapToDTO(student);
    }

    public async Task<StudentDTO?> GetStudentByIdAsync(Guid accountId)
    {
        var student = await _context.StudentInfos
            .Include(s => s.Account)
            .Include(s => s.Department)
            .Include(s => s.Syllabus)
            .Include(s => s.Intake)
            .Include(s => s.StudentStatus)
            .FirstOrDefaultAsync(s => s.AccountId == accountId);

        if (student == null) return null;
        return MapToDTO(student);
    }

    public async Task<string?> CreateStudentAsync(StudentDTO dto)
    {
        // 1. Validate uniqueness
        if (await _context.Accounts.AnyAsync(a => a.Email == dto.Email || a.Username == dto.Email))
            throw new Exception("Email is already registered.");

        if (await _context.StudentInfos.AnyAsync(s => s.StudentCode == dto.StudentCode))
            throw new Exception("Student Code already exists.");

        // 2. Ensure Student Role exists
        var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Student");
        if (studentRole == null)
        {
            studentRole = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = "Student",
                IsActive = true
            };
            _context.Roles.Add(studentRole);
            await _context.SaveChangesAsync();
        }

        // 3. Create Account
        var accountId = Guid.NewGuid();
        var account = new Account
        {
            AccountId = accountId,
            Username = dto.Email,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password ?? throw new ArgumentException("Password is required when creating a student account.")),
            Fullname = dto.Fullname,
            Gender = dto.Gender,
            Dob = dto.Dob.HasValue ? DateTime.SpecifyKind(dto.Dob.Value, DateTimeKind.Utc) : null,
            Phone = dto.Phone,
            Address = dto.Address,
            RoleId = studentRole.RoleId,
            IsActive = dto.IsActive,
            EmailVerified = true // Auto-verify for admin created
        };

        _context.Accounts.Add(account);

        // 4. Create StudentInfo
        var studentInfo = new StudentInfo
        {
            StudentCode = dto.StudentCode,
            AccountId = accountId,
            DepartmentId = dto.DepartmentId,
            SyllabusId = dto.SyllabusId,
            IntakeId = dto.IntakeId,
            StudentStatusId = dto.StudentStatusId
        };

        _context.StudentInfos.Add(studentInfo);
        await _context.SaveChangesAsync();

        return studentInfo.StudentCode;
    }

    public async Task<bool> UpdateStudentAsync(StudentDTO dto)
    {
        var studentInfo = await _context.StudentInfos
            .Include(s => s.Account)
            .FirstOrDefaultAsync(s => s.StudentCode == dto.StudentCode);

        if (studentInfo == null || studentInfo.Account == null) return false;

        // Check email uniqueness if email changed
        if (studentInfo.Account.Email != dto.Email)
        {
            if (await _context.Accounts.AnyAsync(a => a.AccountId != studentInfo.AccountId && (a.Email == dto.Email || a.Username == dto.Email)))
            {
                throw new Exception("Email is already used by another account.");
            }
        }

        // Update Account
        studentInfo.Account.Email = dto.Email;
        studentInfo.Account.Username = dto.Email;
        studentInfo.Account.Fullname = dto.Fullname;
        studentInfo.Account.Gender = dto.Gender;
        studentInfo.Account.Dob = dto.Dob.HasValue ? DateTime.SpecifyKind(dto.Dob.Value, DateTimeKind.Utc) : null;
        studentInfo.Account.Phone = dto.Phone;
        studentInfo.Account.Address = dto.Address;
        studentInfo.Account.IsActive = dto.IsActive;

        // Update StudentInfo
        studentInfo.DepartmentId = dto.DepartmentId;
        studentInfo.SyllabusId = dto.SyllabusId;
        studentInfo.IntakeId = dto.IntakeId;
        studentInfo.StudentStatusId = dto.StudentStatusId;

        _context.Accounts.Update(studentInfo.Account);
        _context.StudentInfos.Update(studentInfo);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateStudentProfileAsync(Guid accountId, StudentProfileUpdateDTO dto)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
        {
            return false;
        }

        account.Phone = dto.Phone;
        account.Gender = dto.Gender;
        account.Dob = dto.Dob.HasValue ? DateTime.SpecifyKind(dto.Dob.Value, DateTimeKind.Utc) : null;
        account.Address = dto.Address;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteStudentAsync(string studentCode)
    {
        var studentInfo = await _context.StudentInfos.FirstOrDefaultAsync(s => s.StudentCode == studentCode);
        if (studentInfo == null) return false;

        var account = await _context.Accounts.FindAsync(studentInfo.AccountId);
        
        // Remove both Note: Make sure Foreign Keys constraints allow this or cascade is setup, otherwise delete StudentInfo first
        _context.StudentInfos.Remove(studentInfo);
        if (account != null)
        {
            _context.Accounts.Remove(account);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetStudentPasswordAsync(string studentCode, string newPassword)
    {
        var studentInfo = await _context.StudentInfos
            .Include(s => s.Account)
            .FirstOrDefaultAsync(s => s.StudentCode == studentCode);

        if (studentInfo == null || studentInfo.Account == null) return false;

        studentInfo.Account.PasswordHash = HashPassword(newPassword);
        _context.Accounts.Update(studentInfo.Account);
        await _context.SaveChangesAsync();

        return true;
    }

    // Lookups
    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetDepartmentsLookupAsync()
    {
        var list = await _context.Departments.Where(x => x.IsActive).OrderBy(x => x.DepartmentNameEng).ToListAsync();
        return list.Select(x => new KeyValuePair<Guid, string>(x.DepartmentId, x.DepartmentNameEng));
    }

    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetSyllabusesLookupAsync(Guid? departmentId = null)
    {
        var query = _context.Syllabuses.Where(x => x.IsActive).AsQueryable();
        
        if (departmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == departmentId.Value);
        }

        var list = await query.OrderBy(x => x.SyllabusName).ToListAsync();
        return list.Select(x => new KeyValuePair<Guid, string>(x.SyllabusId, x.SyllabusName));
    }

    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetIntakesLookupAsync()
    {
        var list = await _context.Intakes.OrderBy(x => x.Name).ToListAsync();
        
        // Auto-seed if empty for demo purposes
        if (!list.Any())
        {
            var seed = new List<Intake>
            {
                new Intake { IntakeId = Guid.NewGuid(), Name = "Fall 2025" },
                new Intake { IntakeId = Guid.NewGuid(), Name = "Spring 2026" },
                new Intake { IntakeId = Guid.NewGuid(), Name = "Summer 2026" }
            };
            _context.Intakes.AddRange(seed);
            await _context.SaveChangesAsync();
            return seed.Select(x => new KeyValuePair<Guid, string>(x.IntakeId, x.Name));
        }

        return list.Select(x => new KeyValuePair<Guid, string>(x.IntakeId, x.Name));
    }

    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetStudentStatusesLookupAsync()
    {
        var list = await _context.StudentStatuses.OrderBy(x => x.Name).ToListAsync();
        
        // Auto-seed if empty for demo purposes
        if (!list.Any())
        {
            var seed = new List<StudentStatus>
            {
                new StudentStatus { StudentStatusId = Guid.NewGuid(), Name = "Studying" },
                new StudentStatus { StudentStatusId = Guid.NewGuid(), Name = "Dropped Out" },
                new StudentStatus { StudentStatusId = Guid.NewGuid(), Name = "Graduated" },
                new StudentStatus { StudentStatusId = Guid.NewGuid(), Name = "Suspended" }
            };
            _context.StudentStatuses.AddRange(seed);
            await _context.SaveChangesAsync();
            return seed.Select(x => new KeyValuePair<Guid, string>(x.StudentStatusId, x.Name));
        }

        return list.Select(x => new KeyValuePair<Guid, string>(x.StudentStatusId, x.Name));
    }

    private StudentDTO MapToDTO(StudentInfo s)
    {
        return new StudentDTO
        {
            AccountId = s.AccountId,
            StudentCode = s.StudentCode,
            Email = s.Account?.Email ?? string.Empty,
            Fullname = s.Account?.Fullname ?? string.Empty,
            Gender = s.Account?.Gender,
            Phone = s.Account?.Phone,
            Dob = s.Account?.Dob,
            Address = s.Account?.Address,
            IsActive = s.Account?.IsActive ?? false,
            DepartmentId = s.DepartmentId,
            SyllabusId = s.SyllabusId,
            IntakeId = s.IntakeId,
            StudentStatusId = s.StudentStatusId,
            
            DepartmentName = s.Department?.DepartmentNameEng,
            SyllabusName = s.Syllabus?.SyllabusName,
            IntakeName = s.Intake?.Name,
            StatusName = s.StudentStatus?.Name
        };
    }

    private const int PbkdfSaltSize = 16;
    private const int PbkdfKeySize = 32;
    private const int PbkdfIterations = 100_000;
    private const byte PbkdfVersion = 1;

    private static string HashPassword(string password)
    {
        byte[] salt = new byte[PbkdfSaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        byte[] key = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(password),
            salt: salt,
            iterations: PbkdfIterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: PbkdfKeySize);

        var result = new byte[1 + PbkdfSaltSize + PbkdfKeySize]; // version + salt + key
        result[0] = PbkdfVersion;
        Buffer.BlockCopy(salt, 0, result, 1, PbkdfSaltSize);
        Buffer.BlockCopy(key, 0, result, 1 + PbkdfSaltSize, PbkdfKeySize);
        return Convert.ToBase64String(result);
    }
}
