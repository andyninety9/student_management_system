using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmsRazor.BLL.Services;
using SmsRazor.DAL.Entities;

namespace SmsRazor.BLL.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task SendMessage(Guid receiverId, string content, int messageType, string? fileName = null)
    {
        try
        {
            var senderIdString = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderIdString) || !Guid.TryParse(senderIdString, out var senderId))
            {
                return; // Unauthorized
            }

            var conversation = await _chatService.GetOrCreateConversationAsync(senderId, receiverId);

            var message = await _chatService.SaveMessageAsync(
                conversation.ConversationId, 
                senderId, 
                content, 
                (MessageType)messageType, 
                fileName);

            // Fetch sender details to send along with message
            var senderName = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown User";

            var messagePayload = new
            {
                messageId = message.MessageId,
                conversationId = conversation.ConversationId,
                senderId = senderId,
                senderName = senderName,
                content = content,
                type = messageType,
                fileName = fileName,
                createdAt = message.CreatedAt
            };

            // Send to the receiver's specific user group (SignalR maps users by ClaimsPrincipal automatically if configured)
            // But for explicit targeting, we broadcast to a conversation-specific group
            await Clients.Group(conversation.ConversationId.ToString()).SendAsync("ReceiveMessage", messagePayload);
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }
    public async Task EditMessage(Guid conversationId, Guid messageId, string newContent)
    {
        try
        {
            var senderIdString = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderIdString) || !Guid.TryParse(senderIdString, out var senderId)) return;

            var message = await _chatService.EditMessageAsync(messageId, senderId, newContent);
            if (message != null)
            {
                await Clients.Group(conversationId.ToString()).SendAsync("MessageEdited", new
                {
                    messageId = message.MessageId,
                    content = message.Content
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error editing message: {ex.Message}");
        }
    }

    public async Task DeleteMessage(Guid conversationId, Guid messageId)
    {
        try
        {
            var senderIdString = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderIdString) || !Guid.TryParse(senderIdString, out var senderId)) return;

            var success = await _chatService.DeleteMessageAsync(messageId, senderId);
            if (success)
            {
                await Clients.Group(conversationId.ToString()).SendAsync("MessageDeleted", new
                {
                    messageId = messageId
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting message: {ex.Message}");
        }
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var userIdString = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            throw new HubException("Unauthorized");
        }

        var conversation = await _chatService.GetConversationAsync(conversationId);
        if (conversation == null ||
            (conversation.UserAccountId1 != userId && conversation.UserAccountId2 != userId))
        {
            throw new HubException("Access denied: you are not a participant of this conversation.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task NotifyTyping(Guid conversationId, bool isTyping)
    {
        var senderIdString = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(senderIdString) || !Guid.TryParse(senderIdString, out var senderId)) return;

        await Clients.Group(conversationId.ToString()).SendAsync("UserTyping", new
        {
            conversationId = conversationId,
            userId = senderId,
            isTyping = isTyping
        });
    }
}
