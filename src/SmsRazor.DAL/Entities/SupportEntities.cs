using System;
using System.ComponentModel.DataAnnotations;

namespace SmsRazor.DAL.Entities;

public class StudentStatus : BaseEntity
{
    [Key]
    public Guid StudentStatusId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Department : BaseEntity
{
    [Key]
    public Guid DepartmentId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string DepartmentNameEng { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string DepartmentNameVI { get; set; } = string.Empty;
    
    public int TotalCredit { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class Intake : BaseEntity
{
    [Key]
    public Guid IntakeId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Syllabus : BaseEntity
{
    [Key]
    public Guid SyllabusId { get; set; }

    [Required]
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }

    [Required]
    [MaxLength(255)]
    public string SyllabusName { get; set; } = string.Empty;
    
    [DataType(DataType.Date)]
    public DateTime EffectiveFrom { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? EffectiveTo { get; set; }
    
    [MaxLength(100)]
    public string Status { get; set; } = "Active";

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
