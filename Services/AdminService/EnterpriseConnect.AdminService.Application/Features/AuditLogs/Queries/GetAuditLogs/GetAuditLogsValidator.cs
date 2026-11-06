using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
