using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQuery : IRequest<Result<(List<AuditLogDto> Items, long TotalCount)>>
{
    public string? CorrelationId { get; set; }
    public string? ServiceName { get; set; }
    public RiskLevel? RiskLevel { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
