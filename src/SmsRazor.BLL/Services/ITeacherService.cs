using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsRazor.BLL.DTOs;

namespace SmsRazor.BLL.Services;

public interface ITeacherService
{
    Task<IEnumerable<TeacherDTO>> GetAllTeachersAsync();
    Task<TeacherDTO?> GetTeacherByCodeAsync(string teacherCode);
    Task<TeacherDTO?> GetTeacherByIdAsync(Guid accountId);
    
    Task<string?> CreateTeacherAsync(TeacherDTO dto);
    Task<bool> UpdateTeacherAsync(TeacherDTO dto);
    Task<bool> DeleteTeacherAsync(string teacherCode);
    Task<bool> ResetTeacherPasswordAsync(string teacherCode, string newPassword);

    // Lookups
    Task<IEnumerable<KeyValuePair<Guid, string>>> GetDepartmentsLookupAsync();
}
