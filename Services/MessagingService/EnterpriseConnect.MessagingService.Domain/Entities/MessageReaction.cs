using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.MessagingService.Domain.Entities;

public class MessageReaction : BaseEntity
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public string Emoji { get; set; } = string.Empty;

    public virtual Message Message { get; set; } = null!;
}
