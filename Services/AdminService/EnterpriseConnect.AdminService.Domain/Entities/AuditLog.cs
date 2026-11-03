using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.AdminService.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid ActorId { get; set; } // Logical FK to IdentityService.User
    public string ActionName { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string? Details { get; set; }
}
