using EnterpriseConnect.AdminService.Domain.Enums;

namespace EnterpriseConnect.AdminService.Application.Common.Models;

public class ToxicViolationDto
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public ViolationType ViolationType { get; set; }
    public SeverityLevel Severity { get; set; }
    public ActionTaken ActionTaken { get; set; }
    public DateTimeOffset DetectedAt { get; set; }
}
