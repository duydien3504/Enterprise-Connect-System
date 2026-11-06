using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditStats;

public class GetAuditStatsQuery : IRequest<Result<SystemDashboardDto>>
{
}
