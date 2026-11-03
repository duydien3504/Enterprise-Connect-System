using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.MessagingService.Domain.Enums;

namespace EnterpriseConnect.MessagingService.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; } // Logical FK to IdentityService.User
    public Guid? ReplyToId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
    public bool IsEdited { get; set; } = false;
    public DateTimeOffset? EditedAt { get; set; }
    public bool IsPinned { get; set; } = false;
    public DateTimeOffset? PinnedAt { get; set; }
    public Guid? PinnedByUserId { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual Message? ReplyTo { get; set; }
    public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
    public virtual ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
    public virtual ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
}
