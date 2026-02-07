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
    public string Name { get; set; } = string.Empty;
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
    public string Name { get; set; } = string.Empty;
}
