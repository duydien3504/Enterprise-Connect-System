using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.IdentityService.Domain.Entities;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
