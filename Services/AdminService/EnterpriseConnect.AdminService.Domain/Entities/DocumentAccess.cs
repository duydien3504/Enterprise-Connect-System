using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.AdminService.Domain.Entities;

public class DocumentAccess : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Guid? RoleId { get; set; } // Logical FK to IdentityService.Role
    public Guid? DepartmentId { get; set; } // Logical FK to IdentityService.Department

    public virtual Document Document { get; set; } = null!;
}
