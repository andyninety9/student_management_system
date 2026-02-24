using System;

namespace SmsRazor.BLL.DTOs;

public class StudentScheduleDTO
{
    public string title { get; set; } = string.Empty;
    public string start { get; set; } = string.Empty;
    public string end { get; set; } = string.Empty;
    public string className { get; set; } = string.Empty;
    public string display { get; set; } = "block";
}
