using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.IdentityService.Domain.Entities;

public class Permission : BaseEntity
{
    public string PermissionName { get; set; } = string.Empty;

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
