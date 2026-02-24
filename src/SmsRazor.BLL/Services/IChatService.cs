using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmsRazor.DAL.Entities;

namespace SmsRazor.BLL.Services;

public interface IChatService
{
    Task<List<Conversation>> GetUserConversationsAsync(Guid userId);
    Task<Conversation?> GetConversationAsync(Guid conversationId);
    Task<Conversation> GetOrCreateConversationAsync(Guid user1Id, Guid user2Id);
    
    Task<Message> SaveMessageAsync(Guid conversationId, Guid senderId, string content, MessageType type, string? fileName = null);
    Task<List<Message>> GetConversationMessagesAsync(Guid conversationId, int skip = 0, int take = 50);

    Task<Message?> EditMessageAsync(Guid messageId, Guid userId, string newContent);
    Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);
}
