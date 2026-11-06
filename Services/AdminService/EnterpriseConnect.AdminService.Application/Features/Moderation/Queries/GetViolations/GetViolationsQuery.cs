using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Queries.GetViolations;

public class GetViolationsQuery : IRequest<Result<(List<ToxicViolationDto> Items, long TotalCount)>>
{
    public ViolationType? ViolationType { get; set; }
    public SeverityLevel? Severity { get; set; }
    public ActionTaken? ActionTaken { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
