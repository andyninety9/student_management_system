using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsRazor.BLL.DTOs;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

using SmsRazor.DAL.Repositories;

namespace SmsRazor.BLL.Services;

public class SyllabusService : ISyllabusService
{
    private readonly IRepository<Syllabus> _syllabusRepository;
    private readonly IRepository<SyllabusCourse> _syllabusCourseRepository;
    private readonly IRepository<Department> _departmentRepository;
    private readonly IRepository<Course> _courseRepository;

    public SyllabusService(
        IRepository<Syllabus> syllabusRepository,
        IRepository<SyllabusCourse> syllabusCourseRepository,
        IRepository<Department> departmentRepository,
        IRepository<Course> courseRepository)
    {
        _syllabusRepository = syllabusRepository;
        _syllabusCourseRepository = syllabusCourseRepository;
        _departmentRepository = departmentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetDepartmentsLookupAsync()
    {
        var departments = await _departmentRepository.Entities
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentNameEng)
            .ToListAsync();

        return departments.Select(d => new KeyValuePair<Guid, string>(d.DepartmentId, $"{d.DepartmentNameEng} / {d.DepartmentNameVI}"));
    }

    public async Task<IEnumerable<KeyValuePair<Guid, string>>> GetCoursesLookupAsync()
    {
        var courses = await _courseRepository.Entities
            .Where(c => c.IsActive)
            .OrderBy(c => c.CourseNameEng)
            .ToListAsync();
        return courses.Select(c => new KeyValuePair<Guid, string>(c.CourseId, c.CourseNameEng));
    }

    public async Task<IEnumerable<SyllabusDTO>> GetAllSyllabiAsync()
    {
        var syllabi = await _syllabusRepository.Entities
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
        var s = await _syllabusRepository.Entities
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.SyllabusId == syllabusId);

        if (s == null) return null;
        
        // Fetch related courses
        var courseIds = await _syllabusCourseRepository.Entities
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

        await _syllabusRepository.AddAsync(syllabus);
        
        if (dto.CourseIds != null && dto.CourseIds.Any())
        {
            var syllabusCourses = dto.CourseIds.Select(cid => new SyllabusCourse
            {
                SyllabusCourseId = Guid.NewGuid(),
                SyllabusId = syllabus.SyllabusId,
                CourseId = cid
            });
            await _syllabusCourseRepository.AddRangeAsync(syllabusCourses);
        }

        await _syllabusRepository.SaveChangesAsync();

        return syllabus.SyllabusId;
    }

    public async Task<bool> UpdateSyllabusAsync(SyllabusDTO dto)
    {
        var syllabus = await _syllabusRepository.GetByIdAsync(dto.SyllabusId);
        if (syllabus == null) return false;

        syllabus.DepartmentId = dto.DepartmentId;
        syllabus.SyllabusName = dto.SyllabusName;
        syllabus.EffectiveFrom = DateTime.SpecifyKind(dto.EffectiveFrom, DateTimeKind.Utc);
        syllabus.EffectiveTo = dto.EffectiveTo.HasValue ? DateTime.SpecifyKind(dto.EffectiveTo.Value, DateTimeKind.Utc) : null;
        syllabus.Status = dto.Status;
        syllabus.Description = dto.Description;
        syllabus.IsActive = dto.IsActive;

        var existingCourses = await _syllabusCourseRepository.Entities.Where(sc => sc.SyllabusId == syllabus.SyllabusId).ToListAsync();
        _syllabusCourseRepository.RemoveRange(existingCourses);

        if (dto.CourseIds != null && dto.CourseIds.Any())
        {
            var newCourses = dto.CourseIds.Select(cid => new SyllabusCourse
            {
                SyllabusCourseId = Guid.NewGuid(),
                SyllabusId = syllabus.SyllabusId,
                CourseId = cid
            });
            await _syllabusCourseRepository.AddRangeAsync(newCourses);
        }

        _syllabusRepository.Update(syllabus);
        await _syllabusRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteSyllabusAsync(Guid syllabusId)
    {
        var syllabus = await _syllabusRepository.GetByIdAsync(syllabusId);
        if (syllabus == null) return false;

        _syllabusRepository.Remove(syllabus);
        await _syllabusRepository.SaveChangesAsync();

        return true;
    }
}
