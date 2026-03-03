using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsRazor.BLL.DTOs;

namespace SmsRazor.BLL.Services;

public interface IStudentService
{
    Task<IEnumerable<StudentDTO>> GetAllStudentsAsync();
    Task<StudentDTO?> GetStudentByCodeAsync(string studentCode);
    Task<StudentDTO?> GetStudentByIdAsync(Guid accountId);
    Task<string?> CreateStudentAsync(StudentDTO dto);
    Task<bool> UpdateStudentAsync(StudentDTO dto);
    Task<bool> UpdateStudentProfileAsync(Guid accountId, StudentProfileUpdateDTO dto);
    Task<bool> DeleteStudentAsync(string studentCode);
    Task<bool> ResetStudentPasswordAsync(string studentCode, string newPassword);
    
    // Lookups
    Task<IEnumerable<KeyValuePair<Guid, string>>> GetDepartmentsLookupAsync();
    Task<IEnumerable<KeyValuePair<Guid, string>>> GetSyllabusesLookupAsync(Guid? departmentId = null);
    Task<IEnumerable<KeyValuePair<Guid, string>>> GetIntakesLookupAsync();
    Task<IEnumerable<KeyValuePair<Guid, string>>> GetStudentStatusesLookupAsync();
}
