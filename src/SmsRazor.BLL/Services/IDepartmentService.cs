using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsRazor.BLL.DTOs;

namespace SmsRazor.BLL.Services;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDTO>> GetAllDepartmentsAsync();
    Task<DepartmentDTO?> GetDepartmentByIdAsync(Guid departmentId);
    Task<Guid> CreateDepartmentAsync(DepartmentDTO dto);
    Task<bool> UpdateDepartmentAsync(DepartmentDTO dto);
    Task<bool> DeleteDepartmentAsync(Guid departmentId);
}
