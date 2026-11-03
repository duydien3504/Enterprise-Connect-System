using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.MessagingService.Domain.Enums;

namespace EnterpriseConnect.MessagingService.Domain.Entities;

public class Conversation : BaseEntity
{
    public ConversationType Type { get; set; } = ConversationType.Direct;
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? LastMessageId { get; set; }
    public DateTimeOffset? LastMessageAt { get; set; }

    public virtual ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<AiSummary> Summaries { get; set; } = new List<AiSummary>();
}
