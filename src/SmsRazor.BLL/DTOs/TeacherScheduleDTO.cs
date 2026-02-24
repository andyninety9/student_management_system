using System;

namespace SmsRazor.BLL.DTOs;

public class TeacherScheduleDTO
{
    public string title { get; set; } = string.Empty;
    public string start { get; set; } = string.Empty;
    public string end { get; set; } = string.Empty;
    public string className { get; set; } = string.Empty;
}
