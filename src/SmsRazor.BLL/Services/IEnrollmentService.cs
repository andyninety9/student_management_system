using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsRazor.BLL.DTOs;

namespace SmsRazor.BLL.Services;

public interface IEnrollmentService
{
    // Fetch courses the student can take based on their Syllabus. Pass termId to compute availability.
    Task<IEnumerable<CourseDTO>> GetStudentSyllabusCoursesAsync(string studentCode, Guid? termId = null);
    
    // Fetch sections for a specific course in a specific term
    Task<IEnumerable<SectionDTO>> GetAvailableSectionsForCourseAsync(Guid courseId, Guid termId);
    
    // Fetch the list of enrollments a student has for a specific term
    Task<IEnumerable<EnrollmentDTO>> GetStudentEnrollmentsByTermAsync(string studentCode, Guid termId);
    
    // Core Registration Actions
    Task<(bool IsSuccess, string ErrorMessage)> RegisterForSectionAsync(string studentCode, Guid sectionId);
    Task<bool> RemoveRegistrationAsync(string studentCode, Guid sectionId);

    // Timetable Integration
    Task<IEnumerable<StudentScheduleDTO>> GetStudentTimetableAsync(string studentCode, Guid termId);
}
