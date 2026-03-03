using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsRazor.BLL.DTOs;

namespace SmsRazor.BLL.Services;

public interface ICourseService
{
    Task<IEnumerable<CourseDTO>> GetAllCoursesAsync();
    Task<CourseDTO?> GetCourseByIdAsync(Guid courseId);
    Task<Guid> CreateCourseAsync(CourseDTO courseDto);
    Task<bool> UpdateCourseAsync(CourseDTO courseDto);
    Task<bool> DeleteCourseAsync(Guid courseId);
    
    // Lookups for UI dropdowns
    Task<IEnumerable<KeyValuePair<Guid, string>>> GetCoursesLookupAsync();
}
