using System;
using System.ComponentModel.DataAnnotations;

namespace SmsRazor.BLL.DTOs;

public class TermDTO
{
    public Guid TermId { get; set; }

    [Required(ErrorMessage = "Term code is required.")]
    [MaxLength(20, ErrorMessage = "Term code cannot exceed 20 characters.")]
    [Display(Name = "Term Code")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Term name is required.")]
    [MaxLength(100, ErrorMessage = "Term name cannot exceed 100 characters.")]
    [Display(Name = "Term Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required.")]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Display(Name = "Status")]
    public bool IsActive { get; set; } = true;
}
