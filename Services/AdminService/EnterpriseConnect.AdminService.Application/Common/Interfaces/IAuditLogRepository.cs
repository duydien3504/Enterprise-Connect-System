using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Domain.Enums;

namespace EnterpriseConnect.AdminService.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<AuditLog> Items, long TotalCount)> GetPagedAsync(
        string? correlationId,
        string? serviceName,
        RiskLevel? riskLevel,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<(long Total, long Safe, long Warning, long Danger, List<string> Services)> GetStatsAsync(
        CancellationToken cancellationToken = default);
}
