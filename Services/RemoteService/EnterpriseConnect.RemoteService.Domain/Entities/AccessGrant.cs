using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.RemoteService.Domain.Enums;

namespace EnterpriseConnect.RemoteService.Domain.Entities;

public class AccessGrant : BaseEntity
{
    public Guid SessionId { get; set; }
    public PermissionType PermissionType { get; set; } = PermissionType.ViewScreen;
    public bool IsGranted { get; set; } = false;

    public virtual RemoteSession Session { get; set; } = null!;
}
