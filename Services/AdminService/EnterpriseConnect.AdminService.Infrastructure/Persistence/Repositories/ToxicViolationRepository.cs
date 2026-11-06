using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Domain.Enums;
using EnterpriseConnect.AdminService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.AdminService.Infrastructure.Persistence.Repositories;

public class ToxicViolationRepository : IToxicViolationRepository
{
    private readonly AdminDbContext _context;

    public ToxicViolationRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ToxicViolation violation, CancellationToken cancellationToken = default)
    {
        await _context.ToxicViolations.AddAsync(violation, cancellationToken);
    }

    public async Task<ToxicViolation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ToxicViolations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(List<ToxicViolation> Items, long TotalCount)> GetPagedAsync(
        ViolationType? type,
        SeverityLevel? severity,
        ActionTaken? action,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ToxicViolations.AsNoTracking().AsQueryable();

        if (type.HasValue)
            query = query.Where(x => x.ViolationType == type.Value);

        if (severity.HasValue)
            query = query.Where(x => x.Severity == severity.Value);

        if (action.HasValue)
            query = query.Where(x => x.ActionTaken == action.Value);

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.DetectedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task UpdateAsync(ToxicViolation violation, CancellationToken cancellationToken = default)
    {
        _context.ToxicViolations.Update(violation);
        return Task.CompletedTask;
    }

    public async Task<long> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ToxicViolations.LongCountAsync(cancellationToken);
    }

    public async Task<long> GetPendingReviewCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ToxicViolations.LongCountAsync(x => x.ActionTaken == ActionTaken.Warned, cancellationToken);
    }
}
