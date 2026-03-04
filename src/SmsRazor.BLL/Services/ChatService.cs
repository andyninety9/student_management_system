using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Entities;

using SmsRazor.DAL.Repositories;

namespace SmsRazor.BLL.Services;

public class ChatService : IChatService
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;

    public ChatService(
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
    }

    public async Task<List<Conversation>> GetUserConversationsAsync(Guid userId)
    {
        return await _conversationRepository.Entities
            .Include(c => c.UserAccount1)
            .Include(c => c.UserAccount2)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1)) // Get latest message for preview
            .Where(c => c.UserAccountId1 == userId || c.UserAccountId2 == userId)
            // Order conversations by the latest message time
            .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.CreatedAt) ?? c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Conversation?> GetConversationAsync(Guid conversationId)
    {
        return await _conversationRepository.Entities
            .Include(c => c.UserAccount1)
            .Include(c => c.UserAccount2)
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
    }

    public async Task<Conversation> GetOrCreateConversationAsync(Guid user1Id, Guid user2Id)
    {
        var existingConversation = await _conversationRepository.Entities
            .FirstOrDefaultAsync(c => 
                (c.UserAccountId1 == user1Id && c.UserAccountId2 == user2Id) ||
                (c.UserAccountId1 == user2Id && c.UserAccountId2 == user1Id));

        if (existingConversation != null)
        {
            return existingConversation;
        }

        var newConversation = new Conversation
        {
            ConversationId = Guid.NewGuid(),
            UserAccountId1 = user1Id,
            UserAccountId2 = user2Id
        };

        await _conversationRepository.AddAsync(newConversation);
        await _conversationRepository.SaveChangesAsync();
        
        return newConversation;
    }

    public async Task<Message> SaveMessageAsync(Guid conversationId, Guid senderId, string content, MessageType type, string? fileName = null)
    {
        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            Type = type,
            FileName = fileName,
            CreatedAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();

        return message;
    }

    public async Task<List<Message>> GetConversationMessagesAsync(Guid conversationId, int skip = 0, int take = 50)
    {
        return await _messageRepository.Entities
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt) // Get newest first
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Message?> EditMessageAsync(Guid messageId, Guid userId, string newContent)
    {
        var message = await _messageRepository.Entities.FirstOrDefaultAsync(m => m.MessageId == messageId && m.SenderId == userId);
        if (message == null || message.Type != MessageType.Text || message.IsDeleted) return null;

        message.Content = newContent;
        message.IsEdited = true;
        message.UpdatedAt = DateTime.UtcNow;

        _messageRepository.Update(message);
        await _messageRepository.SaveChangesAsync();
        return message;
    }

    public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
    {
        var message = await _messageRepository.Entities.FirstOrDefaultAsync(m => m.MessageId == messageId && m.SenderId == userId);
        if (message == null || message.IsDeleted) return false;

        message.IsDeleted = true;
        message.UpdatedAt = DateTime.UtcNow;

        // Optionally clear content for privacy
        if (message.Type == MessageType.Text)
        {
            message.Content = "This message was deleted";
        }
        else if (message.Type == MessageType.File)
        {
            message.Content = "";
            message.FileName = "Deleted File";
        }

        _messageRepository.Update(message);
        await _messageRepository.SaveChangesAsync();
        return true;
    }
}
