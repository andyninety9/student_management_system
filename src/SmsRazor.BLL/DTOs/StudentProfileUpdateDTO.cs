using System;
using System.ComponentModel.DataAnnotations;

namespace SmsRazor.BLL.DTOs;

public class StudentProfileUpdateDTO
{
    public Guid AccountId { get; set; }

    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime? Dob { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }
}
