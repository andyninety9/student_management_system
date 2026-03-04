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

public class DepartmentService : IDepartmentService
{
    private readonly IRepository<Department> _departmentRepository;

    public DepartmentService(IRepository<Department> departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<IEnumerable<DepartmentDTO>> GetAllDepartmentsAsync()
    {
        var departments = await _departmentRepository.Entities
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return departments.Select(d => new DepartmentDTO
        {
            DepartmentId = d.DepartmentId,
            DepartmentNameEng = d.DepartmentNameEng,
            DepartmentNameVI = d.DepartmentNameVI,
            TotalCredit = d.TotalCredit,
            IsActive = d.IsActive
        });
    }

    public async Task<DepartmentDTO?> GetDepartmentByIdAsync(Guid departmentId)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department == null) return null;

        return new DepartmentDTO
        {
            DepartmentId = department.DepartmentId,
            DepartmentNameEng = department.DepartmentNameEng,
            DepartmentNameVI = department.DepartmentNameVI,
            TotalCredit = department.TotalCredit,
            IsActive = department.IsActive
        };
    }

    public async Task<Guid> CreateDepartmentAsync(DepartmentDTO dto)
    {
        var department = new Department
        {
            DepartmentId = Guid.NewGuid(),
            DepartmentNameEng = dto.DepartmentNameEng,
            DepartmentNameVI = dto.DepartmentNameVI,
            TotalCredit = dto.TotalCredit,
            IsActive = dto.IsActive
        };

        await _departmentRepository.AddAsync(department);
        await _departmentRepository.SaveChangesAsync();
        
        return department.DepartmentId;
    }

    public async Task<bool> UpdateDepartmentAsync(DepartmentDTO dto)
    {
        var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);
        if (department == null) return false;

        department.DepartmentNameEng = dto.DepartmentNameEng;
        department.DepartmentNameVI = dto.DepartmentNameVI;
        department.TotalCredit = dto.TotalCredit;
        department.IsActive = dto.IsActive;

        _departmentRepository.Update(department);
        await _departmentRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteDepartmentAsync(Guid departmentId)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department == null) return false;

        _departmentRepository.Remove(department);
        await _departmentRepository.SaveChangesAsync();

        return true;
    }
}
