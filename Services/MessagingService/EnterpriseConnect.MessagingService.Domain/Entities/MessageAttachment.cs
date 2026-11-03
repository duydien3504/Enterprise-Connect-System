using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.MessagingService.Domain.Entities;

public class MessageAttachment : BaseEntity
{
    public Guid MessageId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;

    public virtual Message Message { get; set; } = null!;
}
