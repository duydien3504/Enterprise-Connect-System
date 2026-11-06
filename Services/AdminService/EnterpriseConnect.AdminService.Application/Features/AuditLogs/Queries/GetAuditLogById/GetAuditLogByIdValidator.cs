using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditLogById;

public class GetAuditLogByIdValidator : AbstractValidator<GetAuditLogByIdQuery>
{
    public GetAuditLogByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
