using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Commands.RecordViolation;

public class RecordViolationCommand : IRequest<Result<ToxicViolationDto>>
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public ViolationType ViolationType { get; set; } = ViolationType.Profanity;
    public SeverityLevel Severity { get; set; } = SeverityLevel.Low;
    public ActionTaken ActionTaken { get; set; } = ActionTaken.Warned;
}
