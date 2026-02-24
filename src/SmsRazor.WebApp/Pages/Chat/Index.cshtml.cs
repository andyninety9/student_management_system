using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmsRazor.BLL.Services;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

namespace SmsRazor.WebApp.Pages.Chat;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IChatService _chatService;
    private readonly SmsDbContext _context;

    public IndexModel(IChatService chatService, SmsDbContext context)
    {
        _chatService = chatService;
        _context = context;
    }

    public Guid CurrentUserId { get; set; }
    public string CurrentUserName { get; set; } = string.Empty;
    public string CurrentUserAvatar { get; set; } = string.Empty;

    public List<Conversation> Conversations { get; set; } = new List<Conversation>();

    [BindProperty(SupportsGet = true)]
    public Guid? ActiveConversationId { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid? NewContactId { get; set; }

    public Conversation? ActiveConversation { get; set; }
    public List<Message> ActiveMessages { get; set; } = new List<Message>();

    public Account? ActiveContact { get; set; }

    // List of contacts to start a new chat with 
    // (If student: list of their teachers. If teacher: list of their students)
    public List<Account> AvailableContacts { get; set; } = new List<Account>();

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return RedirectToPage("/Auth/Login");
        }

        CurrentUserId = userId;
        CurrentUserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
        CurrentUserAvatar = User.FindFirst("AvatarUrl")?.Value ?? "/images/default-avatar.png";

        // Load existing conversations
        Conversations = await _chatService.GetUserConversationsAsync(CurrentUserId);

        // If user wants to start a new chat, force create or get it
        if (NewContactId.HasValue && NewContactId.Value != Guid.Empty && NewContactId.Value != CurrentUserId)
        {
            var newConv = await _chatService.GetOrCreateConversationAsync(CurrentUserId, NewContactId.Value);
            ActiveConversationId = newConv.ConversationId;
            return RedirectToPage(new { ActiveConversationId = newConv.ConversationId });
        }

        // Load Active Conversation Details
        if (ActiveConversationId.HasValue && ActiveConversationId.Value != Guid.Empty)
        {
            ActiveConversation = await _chatService.GetConversationAsync(ActiveConversationId.Value);
            if (ActiveConversation != null)
            {
                // Ensure the user is part of the conversation
                if (ActiveConversation.UserAccountId1 != CurrentUserId && ActiveConversation.UserAccountId2 != CurrentUserId)
                {
                    return Forbid();
                }

                // Determine the other person in the chat
                ActiveContact = ActiveConversation.UserAccountId1 == CurrentUserId ? ActiveConversation.UserAccount2 : ActiveConversation.UserAccount1;
                
                // Get message history
                ActiveMessages = await _chatService.GetConversationMessagesAsync(ActiveConversationId.Value, 0, 100);
                ActiveMessages.Reverse(); // Display chronological top-to-bottom
            }
        }
        else if (Conversations.Any())
        {
            // Default select the most recent conversation if none is specified
            return RedirectToPage(new { ActiveConversationId = Conversations.First().ConversationId });
        }

        // Logic to populate "AvailableContacts" to start new discussions can be implemented here
        // E.g. finding all teachers for a student's enrolled courses. 
        // For simplicity, we can fetch all Active Accounts except self.
        AvailableContacts = _context.Accounts
            .Where(a => a.AccountId != CurrentUserId && a.IsActive)
            .OrderBy(a => a.Fullname ?? a.Username)
            .ToList();

        return Page();
    }
}
