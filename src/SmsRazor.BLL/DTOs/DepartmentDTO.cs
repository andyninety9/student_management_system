using System;
using System.ComponentModel.DataAnnotations;

namespace SmsRazor.BLL.DTOs;

public class DepartmentDTO
{
    public Guid DepartmentId { get; set; }
    
    [Required]
    [MaxLength(255)]
    [Display(Name = "English Name")]
    public string DepartmentNameEng { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    [Display(Name = "Vietnamese Name")]
    public string DepartmentNameVI { get; set; } = string.Empty;
    
    [Required]
    [Range(0, 500)]
    [Display(Name = "Total Credits")]
    public int TotalCredit { get; set; }
    
    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
