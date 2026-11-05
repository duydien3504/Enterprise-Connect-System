using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.RemoteService.Domain.Enums;

namespace EnterpriseConnect.RemoteService.Domain.Entities;

public class RemoteSession : BaseEntity
{
    public string SessionCode { get; set; } = string.Empty;
    public Guid RequesterId { get; set; }
    public Guid? SupporterId { get; set; }
    public RemoteSessionStatus Status { get; set; } = RemoteSessionStatus.Created;
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? ConnectedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public int DurationSeconds { get; set; }
    public string? EndReason { get; set; }
    public bool IsSingleUse { get; set; } = true;

    public virtual ICollection<AccessGrant> AccessGrants { get; set; } = new List<AccessGrant>();
}
