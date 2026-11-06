using EnterpriseConnect.AdminService.Domain.Enums;

namespace EnterpriseConnect.AdminService.Api.DTOs;

public class RecordViolationRequest
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public ViolationType ViolationType { get; set; } = ViolationType.Profanity;
    public SeverityLevel Severity { get; set; } = SeverityLevel.Low;
    public ActionTaken ActionTaken { get; set; } = ActionTaken.Warned;
}
