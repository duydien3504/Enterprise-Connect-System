using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.RemoteService.Domain.Enums;

namespace EnterpriseConnect.RemoteService.Domain.Entities;

public class RemoteSession : BaseEntity
{
    public string SessionCode { get; set; } = string.Empty;
    public Guid RequesterId { get; set; } // Logical FK to IdentityService.User
    public Guid? SupporterId { get; set; } // Logical FK to IdentityService.User
    public RemoteSessionStatus Status { get; set; } = RemoteSessionStatus.Created;
    public DateTimeOffset? ExpiresAt { get; set; }

    public virtual ICollection<AccessGrant> AccessGrants { get; set; } = new List<AccessGrant>();
}
