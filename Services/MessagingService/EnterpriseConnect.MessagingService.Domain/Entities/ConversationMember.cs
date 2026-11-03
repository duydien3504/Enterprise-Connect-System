using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.MessagingService.Domain.Enums;

namespace EnterpriseConnect.MessagingService.Domain.Entities;

public class ConversationMember : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; } // Logical FK to IdentityService.User
    public MemberRole MemberRole { get; set; } = MemberRole.Member;
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Conversation Conversation { get; set; } = null!;
}
