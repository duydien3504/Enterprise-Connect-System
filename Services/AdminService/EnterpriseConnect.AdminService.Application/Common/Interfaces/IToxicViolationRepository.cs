using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Domain.Enums;

namespace EnterpriseConnect.AdminService.Application.Common.Interfaces;

public interface IToxicViolationRepository
{
    Task AddAsync(ToxicViolation violation, CancellationToken cancellationToken = default);
    Task<ToxicViolation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<ToxicViolation> Items, long TotalCount)> GetPagedAsync(
        ViolationType? type,
        SeverityLevel? severity,
        ActionTaken? action,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(ToxicViolation violation, CancellationToken cancellationToken = default);
    Task<long> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<long> GetPendingReviewCountAsync(CancellationToken cancellationToken = default);
}
