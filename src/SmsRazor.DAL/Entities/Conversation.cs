using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmsRazor.DAL.Entities;

[Table("Conversations")]
public class Conversation : BaseEntity
{
    [Key]
    public Guid ConversationId { get; set; } = Guid.NewGuid();

    // The two participants in this 1-on-1 chat
    public Guid UserAccountId1 { get; set; }
    [ForeignKey("UserAccountId1")]
    public Account? UserAccount1 { get; set; }

    public Guid UserAccountId2 { get; set; }
    [ForeignKey("UserAccountId2")]
    public Account? UserAccount2 { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
