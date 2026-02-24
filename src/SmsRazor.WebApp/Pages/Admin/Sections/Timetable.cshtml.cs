using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmsRazor.BLL.DTOs;
using SmsRazor.BLL.Services;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

namespace SmsRazor.WebApp.Pages.Admin.Sections
{
    [Authorize(Roles = "Admin,Root")]
    public class TimetableModel : PageModel
    {
        private readonly ISectionService _sectionService;
        private readonly SmsDbContext _dbContext;

        public TimetableModel(ISectionService sectionService, SmsDbContext dbContext)
        {
            _sectionService = sectionService;
            _dbContext = dbContext;
        }

        public SectionDTO? SectionDto { get; set; }
        public List<AcademicCalendar> Calendars { get; set; } = new List<AcademicCalendar>();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            SectionDto = await _sectionService.GetSectionByIdAsync(id);
            if (SectionDto == null) return NotFound();

            // Directly fetch calendar slots order by date for nice display
            Calendars = await _dbContext.AcademicCalendars
                .Where(c => c.SectionId == id)
                .OrderBy(c => c.StudyDate)
                .ToListAsync();

            return Page();
        }
    }
}
