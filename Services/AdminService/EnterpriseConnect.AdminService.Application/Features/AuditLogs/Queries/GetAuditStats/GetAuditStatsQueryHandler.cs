using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditStats;

public class GetAuditStatsQueryHandler : IRequestHandler<GetAuditStatsQuery, Result<SystemDashboardDto>>
{
    private readonly IAdminUnitOfWork _unitOfWork;

    public GetAuditStatsQueryHandler(IAdminUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SystemDashboardDto>> Handle(GetAuditStatsQuery request, CancellationToken cancellationToken)
    {
        var (totalLogs, safeLogs, warningLogs, dangerLogs, services) = 
            await _unitOfWork.AuditLogs.GetStatsAsync(cancellationToken);

        var totalDocs = await _unitOfWork.Documents.GetTotalCountAsync(cancellationToken);
        var indexedDocs = await _unitOfWork.Documents.GetIndexedCountAsync(cancellationToken);
        var totalViolations = await _unitOfWork.ToxicViolations.GetTotalCountAsync(cancellationToken);
        var pendingViolations = await _unitOfWork.ToxicViolations.GetPendingReviewCountAsync(cancellationToken);

        var dashboard = new SystemDashboardDto
        {
            TotalAuditLogs = totalLogs,
            SafeLogsCount = safeLogs,
            WarningLogsCount = warningLogs,
            DangerLogsCount = dangerLogs,
            TotalDocuments = totalDocs,
            IndexedDocuments = indexedDocs,
            TotalViolations = totalViolations,
            PendingViolations = pendingViolations,
            ActiveServices = services,
            GeneratedAt = DateTimeOffset.UtcNow
        };

        return Result<SystemDashboardDto>.Success(dashboard);
    }
}
