using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.MessagingService.Domain.Entities;

public class AiSummary : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid? RequestedByUserId { get; set; }
    public Guid? FromMessageId { get; set; }
    public Guid? ToMessageId { get; set; }
    public string SummaryText { get; set; } = string.Empty;
    public string? KeyPoints { get; set; }
    public string? Decisions { get; set; }
    public string? ActionItems { get; set; }
    public string? Participants { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
}
