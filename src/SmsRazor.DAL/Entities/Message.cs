using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

public enum MessageType
{
    Text = 0,
    File = 1,
    Sticker = 2
}

[Table("Messages")]
public class Message : BaseEntity
{
    [Key]
    public Guid MessageId { get; set; } = Guid.NewGuid();

    public Guid ConversationId { get; set; }
    [ForeignKey("ConversationId")]
    public Conversation? Conversation { get; set; }

    public Guid SenderId { get; set; }
    [ForeignKey("SenderId")]
    public Account? Sender { get; set; }

    // Can be the text content, file URL, or sticker ID
    [Required]
    public string Content { get; set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.Text;

    [MaxLength(255)]
    public string? FileName { get; set; } // Only used if Type == File

    public bool IsDeleted { get; set; } = false;
    public bool IsEdited { get; set; } = false;
}
