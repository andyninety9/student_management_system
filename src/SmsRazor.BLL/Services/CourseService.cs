using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsRazor.BLL.DTOs;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

namespace SmsRazor.BLL.Services;

public class CourseService : ICourseService
{
    private readonly SmsDbContext _context;

    public CourseService(SmsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetCoursesLookupAsync()
    {
        var courses = await _context.Courses
            .Where(c => c.IsActive)
            .OrderBy(c => c.CourseNameEng)
            .ToListAsync();
        return courses.Select(c => new KeyValuePair<Guid, string>(c.CourseId, c.CourseNameEng));
    }



    public async Task<IEnumerable<CourseDTO>> GetAllCoursesAsync()
    {
        var courses = await _context.Courses
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return courses.Select(c => new CourseDTO
        {
            CourseId = c.CourseId,
            CourseNameEng = c.CourseNameEng,
            CourseNameVI = c.CourseNameVI,
            CreditNumber = c.CreditNumber,
            TuitionFee = c.TuitionFee,
            IsActive = c.IsActive,
            CourseRequiredID = c.CourseRequiredID
        });
    }

    public async Task<CourseDTO?> GetCourseByIdAsync(Guid courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Prerequisites)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

        if (course == null) return null;

        return new CourseDTO
        {
            CourseId = course.CourseId,
            CourseNameEng = course.CourseNameEng,
            CourseNameVI = course.CourseNameVI,
            CreditNumber = course.CreditNumber,
            TuitionFee = course.TuitionFee,
            IsActive = course.IsActive,
            CourseRequiredID = course.CourseRequiredID,
            PrerequisiteCourseIds = course.Prerequisites.Select(p => p.PrerequisiteCourseId).ToList()
        };
    }

    public async Task<Guid> CreateCourseAsync(CourseDTO dto)
    {
        var course = new Course
        {
            CourseId = Guid.NewGuid(),
            CourseNameEng = dto.CourseNameEng,
            CourseNameVI = dto.CourseNameVI,
            CreditNumber = dto.CreditNumber,
            TuitionFee = dto.TuitionFee,
            IsActive = dto.IsActive,
            CourseRequiredID = dto.CourseRequiredID,
            Prerequisites = dto.PrerequisiteCourseIds != null ? dto.PrerequisiteCourseIds.Select(pid => new CoursePrerequisite
            {
                CoursePrerequisiteId = Guid.NewGuid(),
                PrerequisiteCourseId = pid
            }).ToList() : new List<CoursePrerequisite>()
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return course.CourseId;
    }

    public async Task<bool> UpdateCourseAsync(CourseDTO dto)
    {
        var course = await _context.Courses
            .Include(c => c.Prerequisites)
            .FirstOrDefaultAsync(c => c.CourseId == dto.CourseId);
            
        if (course == null) return false;

        course.CourseNameEng = dto.CourseNameEng;
        course.CourseNameVI = dto.CourseNameVI;
        course.CreditNumber = dto.CreditNumber;
        course.TuitionFee = dto.TuitionFee;
        course.IsActive = dto.IsActive;
        course.CourseRequiredID = dto.CourseRequiredID;
        
        // Update prerequisites
        var existingPrerequisites = course.Prerequisites.ToList();
        _context.CoursePrerequisites.RemoveRange(existingPrerequisites);
        course.Prerequisites.Clear();
        
        if (dto.PrerequisiteCourseIds != null && dto.PrerequisiteCourseIds.Any())
        {
            var newPrereqs = dto.PrerequisiteCourseIds.Select(pid => new CoursePrerequisite
            {
                CoursePrerequisiteId = Guid.NewGuid(),
                CourseId = course.CourseId,
                PrerequisiteCourseId = pid
            }).ToList();
            
            _context.CoursePrerequisites.AddRange(newPrereqs);
            
            // To ensure navigation property is up-to-date in memory
            foreach(var np in newPrereqs)
            {
                course.Prerequisites.Add(np);
            }
        }
        
        // UpdatedAt is handled automatically by DbContext SaveChangesAsync override
        // Removed "_context.Courses.Update(course);" because it marks un-tracked navigation properties as Modified instead of Added, causing a DbUpdateConcurrencyException on newly generated primary keys.
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCourseAsync(Guid courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return false;

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return true;
    }
}
