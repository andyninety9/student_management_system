using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsRazor.BLL.DTOs;

namespace SmsRazor.BLL.Services;

public interface ISyllabusService
{
    Task<IEnumerable<SyllabusDTO>> GetAllSyllabiAsync();
    Task<SyllabusDTO?> GetSyllabusByIdAsync(Guid syllabusId);
    Task<Guid> CreateSyllabusAsync(SyllabusDTO syllabusDto);
    Task<bool> UpdateSyllabusAsync(SyllabusDTO syllabusDto);
    Task<bool> DeleteSyllabusAsync(Guid syllabusId);
    
    // Departments (for dropdown selection)
    Task<IEnumerable<KeyValuePair<Guid, string>>> GetDepartmentsLookupAsync();

    // Courses (for multi-select)
    Task<IEnumerable<KeyValuePair<Guid, string>>> GetCoursesLookupAsync();
}
