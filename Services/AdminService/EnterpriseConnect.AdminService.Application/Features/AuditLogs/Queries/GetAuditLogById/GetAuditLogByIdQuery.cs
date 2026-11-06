using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditLogById;

public class GetAuditLogByIdQuery : IRequest<Result<AuditLogDto>>
{
    public Guid Id { get; set; }

    public GetAuditLogByIdQuery(Guid id)
    {
        Id = id;
    }
}
