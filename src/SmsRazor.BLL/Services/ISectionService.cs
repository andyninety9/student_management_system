using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsRazor.BLL.DTOs;

namespace SmsRazor.BLL.Services;

public interface ISectionService
{
    Task<IEnumerable<SectionDTO>> GetAllSectionsAsync();
    Task<SectionDTO?> GetSectionByIdAsync(Guid sectionId);
    
    /// <summary>
    /// Creates a section, assigns a teacher, and generates the timetable (Academic Calendars)
    /// </summary>
    Task<Guid> CreateSectionAndScheduleAsync(ScheduleGenerationDTO dto);
    
    Task<bool> DeleteSectionAsync(Guid sectionId);

    // Lookups
    Task<IEnumerable<KeyValuePair<Guid, string>>> GetTermsLookupAsync();
    Task<IEnumerable<KeyValuePair<Guid, string>>> GetCoursesLookupAsync();
    Task<IEnumerable<KeyValuePair<string, string>>> GetTeachersLookupAsync();
    
    // Schedule visualization
    Task<IEnumerable<TeacherScheduleDTO>> GetTeacherScheduleAsync(string teacherCode, Guid termId);
}
