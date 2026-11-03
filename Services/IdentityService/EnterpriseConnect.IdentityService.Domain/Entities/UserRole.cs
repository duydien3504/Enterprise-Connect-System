using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.IdentityService.Domain.Entities;

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
