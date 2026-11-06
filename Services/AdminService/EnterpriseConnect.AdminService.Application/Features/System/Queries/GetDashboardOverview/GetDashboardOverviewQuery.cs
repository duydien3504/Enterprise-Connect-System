using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.System.Queries.GetDashboardOverview;

public class GetDashboardOverviewQuery : IRequest<Result<SystemDashboardDto>>
{
}
