using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using SmsRazor.BLL.Helpers;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Pages.Student.Tuition;

[Authorize(Roles = "Student")]
public class IndexModel : PageModel
{
    private readonly ITuitionService _tuitionService;
    private readonly ISectionService _sectionService;
    private readonly IConfiguration _config;

    public IndexModel(ITuitionService tuitionService, ISectionService sectionService, IConfiguration config)
    {
        _tuitionService = tuitionService;
        _sectionService = sectionService;
        _config = config;
    }

    [BindProperty(SupportsGet = true)]
    public Guid SelectedTermId { get; set; }

    public SelectList TermsList { get; set; } = default!;
    public TuitionSummaryDTO Summary { get; set; } = new TuitionSummaryDTO();
    public string ErrorMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var termsLookup = await _sectionService.GetTermsLookupAsync();
        TermsList = new SelectList(termsLookup, "Key", "Value");

        if (SelectedTermId != Guid.Empty)
        {
            var studentCodeClaim = User.FindFirst("StudentCode")?.Value;
            if (!string.IsNullOrEmpty(studentCodeClaim))
            {
                Summary = await _tuitionService.GetTuitionSummaryAsync(studentCodeClaim, SelectedTermId);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostPayAsync(Guid termId, decimal amount)
    {
        if (amount <= 0)
        {
            ErrorMessage = "Cannot process payment for 0 VND.";
            return await OnGetAsync();
        }

        var studentCodeClaim = User.FindFirst("StudentCode")?.Value;
        if (string.IsNullOrEmpty(studentCodeClaim)) return Unauthorized();

        var ipAddress = Utils.GetIpAddress(HttpContext);
        
        try
        {
            var paymentUrl = await _tuitionService.CreatePaymentUrlAsync(studentCodeClaim, termId, amount, ipAddress, _config);
            return Redirect(paymentUrl);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error initiating payment: " + ex.Message;
            SelectedTermId = termId;
            return await OnGetAsync();
        }
    }
}
