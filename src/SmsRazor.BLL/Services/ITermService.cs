using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsRazor.BLL.DTOs;

namespace SmsRazor.BLL.Services;

public interface ITermService
{
    Task<IEnumerable<TermDTO>> GetAllTermsAsync();
    Task<TermDTO?> GetTermByIdAsync(Guid termId);
    Task<Guid> CreateTermAsync(TermDTO dto);
    Task<bool> UpdateTermAsync(TermDTO dto);
    Task<bool> DeleteTermAsync(Guid termId);
}
