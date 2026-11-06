using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Domain.Enums;
using EnterpriseConnect.AdminService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.AdminService.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AdminDbContext _context;

    public AuditLogRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(log, cancellationToken);
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(List<AuditLog> Items, long TotalCount)> GetPagedAsync(
        string? correlationId,
        string? serviceName,
        RiskLevel? riskLevel,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(correlationId))
            query = query.Where(x => x.CorrelationId == correlationId);

        if (!string.IsNullOrWhiteSpace(serviceName))
            query = query.Where(x => x.ServiceName == serviceName);

        if (riskLevel.HasValue)
            query = query.Where(x => x.RiskLevel == riskLevel.Value);

        if (from.HasValue)
            query = query.Where(x => x.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.Timestamp <= to.Value);

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.Timestamp)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<(long Total, long Safe, long Warning, long Danger, List<string> Services)> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var total = await _context.AuditLogs.LongCountAsync(cancellationToken);
        var safe = await _context.AuditLogs.LongCountAsync(x => x.RiskLevel == RiskLevel.Safe, cancellationToken);
        var warning = await _context.AuditLogs.LongCountAsync(x => x.RiskLevel == RiskLevel.Warning, cancellationToken);
        var danger = await _context.AuditLogs.LongCountAsync(x => x.RiskLevel == RiskLevel.Danger, cancellationToken);
        var services = await _context.AuditLogs.Select(x => x.ServiceName).Distinct().ToListAsync(cancellationToken);

        return (total, safe, warning, danger, services);
    }
}
