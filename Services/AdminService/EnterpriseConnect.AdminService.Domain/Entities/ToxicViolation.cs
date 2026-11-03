using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.AdminService.Domain.Enums;

namespace EnterpriseConnect.AdminService.Domain.Entities;

public class ToxicViolation : BaseEntity
{
    public Guid MessageId { get; set; } // Logical FK to MessagingService.Message
    public Guid SenderId { get; set; } // Logical FK to IdentityService.User
    public ViolationType ViolationType { get; set; } = ViolationType.Profanity;
    public SeverityLevel Severity { get; set; } = SeverityLevel.Low;
    public ActionTaken ActionTaken { get; set; } = ActionTaken.Warned;
    public DateTimeOffset DetectedAt { get; set; } = DateTimeOffset.UtcNow;
}
