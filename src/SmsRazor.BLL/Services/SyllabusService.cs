using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsRazor.BLL.DTOs;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

namespace SmsRazor.BLL.Services;

public class SyllabusService : ISyllabusService
{
    private readonly SmsDbContext _context;

    public SyllabusService(SmsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetDepartmentsLookupAsync()
    {
        var departments = await _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentNameEng)
            .ToListAsync();

        return departments.Select(d => new KeyValuePair<Guid, string>(d.DepartmentId, $"{d.DepartmentNameEng} / {d.DepartmentNameVI}"));
    }

    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetCoursesLookupAsync()
    {
        var courses = await _context.Courses
            .Where(c => c.IsActive)
            .OrderBy(c => c.CourseNameEng)
            .ToListAsync();
        return courses.Select(c => new KeyValuePair<Guid, string>(c.CourseId, c.CourseNameEng));
    }

    public async Task<IEnumerable<SyllabusDTO>> GetAllSyllabiAsync()
    {
        var syllabi = await _context.Syllabuses
            .Include(s => s.Department)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return syllabi.Select(s => new SyllabusDTO
        {
            SyllabusId = s.SyllabusId,
            DepartmentId = s.DepartmentId,
            DepartmentName = s.Department?.DepartmentNameEng,
            SyllabusName = s.SyllabusName,
            EffectiveFrom = s.EffectiveFrom,
            EffectiveTo = s.EffectiveTo,
            Status = s.Status,
            Description = s.Description,
            IsActive = s.IsActive
        });
    }

    public async Task<SyllabusDTO?> GetSyllabusByIdAsync(Guid syllabusId)
    {
        var s = await _context.Syllabuses
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.SyllabusId == syllabusId);

        if (s == null) return null;
        
        // Fetch related courses
        var courseIds = await _context.SyllabusCourses
            .Where(sc => sc.SyllabusId == syllabusId)
            .Select(sc => sc.CourseId)
            .ToListAsync();

        return new SyllabusDTO
        {
            SyllabusId = s.SyllabusId,
            DepartmentId = s.DepartmentId,
            DepartmentName = s.Department?.DepartmentNameEng,
            SyllabusName = s.SyllabusName,
            EffectiveFrom = s.EffectiveFrom,
            EffectiveTo = s.EffectiveTo,
            Status = s.Status,
            Description = s.Description,
            IsActive = s.IsActive,
            CourseIds = courseIds
        };
    }

    public async Task<Guid> CreateSyllabusAsync(SyllabusDTO dto)
    {
        var syllabus = new Syllabus
        {
            SyllabusId = Guid.NewGuid(),
            DepartmentId = dto.DepartmentId,
            SyllabusName = dto.SyllabusName,
            EffectiveFrom = DateTime.SpecifyKind(dto.EffectiveFrom, DateTimeKind.Utc),
            EffectiveTo = dto.EffectiveTo.HasValue ? DateTime.SpecifyKind(dto.EffectiveTo.Value, DateTimeKind.Utc) : null,
            Status = dto.Status,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

        _context.Syllabuses.Add(syllabus);
        
        if (dto.CourseIds != null && dto.CourseIds.Any())
        {
            var syllabusCourses = dto.CourseIds.Select(cid => new SyllabusCourse
            {
                SyllabusCourseId = Guid.NewGuid(),
                SyllabusId = syllabus.SyllabusId,
                CourseId = cid
            });
            _context.SyllabusCourses.AddRange(syllabusCourses);
        }

        await _context.SaveChangesAsync();

        return syllabus.SyllabusId;
    }

    public async Task<bool> UpdateSyllabusAsync(SyllabusDTO dto)
    {
        var syllabus = await _context.Syllabuses.FindAsync(dto.SyllabusId);
        if (syllabus == null) return false;

        syllabus.DepartmentId = dto.DepartmentId;
        syllabus.SyllabusName = dto.SyllabusName;
        syllabus.EffectiveFrom = DateTime.SpecifyKind(dto.EffectiveFrom, DateTimeKind.Utc);
        syllabus.EffectiveTo = dto.EffectiveTo.HasValue ? DateTime.SpecifyKind(dto.EffectiveTo.Value, DateTimeKind.Utc) : null;
        syllabus.Status = dto.Status;
        syllabus.Description = dto.Description;
        syllabus.IsActive = dto.IsActive;

        var existingCourses = await _context.SyllabusCourses.Where(sc => sc.SyllabusId == syllabus.SyllabusId).ToListAsync();
        _context.SyllabusCourses.RemoveRange(existingCourses);

        if (dto.CourseIds != null && dto.CourseIds.Any())
        {
            var newCourses = dto.CourseIds.Select(cid => new SyllabusCourse
            {
                SyllabusCourseId = Guid.NewGuid(),
                SyllabusId = syllabus.SyllabusId,
                CourseId = cid
            });
            _context.SyllabusCourses.AddRange(newCourses);
        }

        _context.Syllabuses.Update(syllabus);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteSyllabusAsync(Guid syllabusId)
    {
        var syllabus = await _context.Syllabuses.FindAsync(syllabusId);
        if (syllabus == null) return false;

        _context.Syllabuses.Remove(syllabus);
        await _context.SaveChangesAsync();

        return true;
    }
}
