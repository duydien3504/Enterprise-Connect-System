using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.AuditLogs.Commands.RecordAuditLog;

public class RecordAuditLogValidator : AbstractValidator<RecordAuditLogCommand>
{
    public RecordAuditLogValidator()
    {
        RuleFor(x => x.CorrelationId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ServiceName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ActionName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.TargetEntity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.IpAddress).NotEmpty().MaximumLength(50);
        RuleFor(x => x.HttpMethod).NotEmpty().MaximumLength(10);
        RuleFor(x => x.EndpointUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.StatusCode).InclusiveBetween(100, 599);
    }
}
