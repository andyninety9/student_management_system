using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsRazor.BLL.DTOs;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

namespace SmsRazor.BLL.Services;

public class TermService : ITermService
{
    private readonly SmsDbContext _context;

    public TermService(SmsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TermDTO>> GetAllTermsAsync()
    {
        var terms = await _context.Terms
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();

        return terms.Select(MapToDto);
    }

    public async Task<TermDTO?> GetTermByIdAsync(Guid termId)
    {
        var term = await _context.Terms.FindAsync(termId);
        if (term == null) return null;
        
        return MapToDto(term);
    }

    public async Task<Guid> CreateTermAsync(TermDTO dto)
    {
        if (await _context.Terms.AnyAsync(t => t.Code == dto.Code))
            throw new Exception("Term code already exists.");

        if (dto.EndDate <= dto.StartDate)
            throw new Exception("End date must be after start date.");

        var term = new Term
        {
            TermId = Guid.NewGuid(),
            Code = dto.Code,
            Name = dto.Name,
            // Ensure dates are stored as UTC
            StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc),
            IsActive = dto.IsActive
        };

        _context.Terms.Add(term);
        await _context.SaveChangesAsync();

        return term.TermId;
    }

    public async Task<bool> UpdateTermAsync(TermDTO dto)
    {
        var term = await _context.Terms.FindAsync(dto.TermId);
        if (term == null) return false;

        if (term.Code != dto.Code && await _context.Terms.AnyAsync(t => t.Code == dto.Code))
            throw new Exception("Term code already exists.");

        if (dto.EndDate <= dto.StartDate)
            throw new Exception("End date must be after start date.");

        term.Code = dto.Code;
        term.Name = dto.Name;
        term.StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
        term.EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);
        term.IsActive = dto.IsActive;

        _context.Terms.Update(term);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteTermAsync(Guid termId)
    {
        var term = await _context.Terms.FindAsync(termId);
        if (term == null) return false;

        // Optionally check if term is used in sections or calendars before deleting
        var isUsed = await _context.AcademicCalendars.AnyAsync(c => c.TermId == termId);
        if (isUsed) throw new Exception("Cannot delete a term that has generated academic calendars. Please remove associated classes first.");

        _context.Terms.Remove(term);
        await _context.SaveChangesAsync();

        return true;
    }

    private static TermDTO MapToDto(Term term)
    {
        return new TermDTO
        {
            TermId = term.TermId,
            Code = term.Code,
            Name = term.Name,
            StartDate = term.StartDate,
            EndDate = term.EndDate,
            IsActive = term.IsActive
        };
    }
}
