using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;
using EnterpriseConnect.RemoteService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.RemoteService.Infrastructure.Persistence.Repositories;

public class RemoteSessionRepository : IRemoteSessionRepository
{
    private readonly RemoteDbContext _context;

    public RemoteSessionRepository(RemoteDbContext context)
    {
        _context = context;
    }

    public async Task<RemoteSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RemoteSessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<RemoteSession?> GetByIdWithGrantsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RemoteSessions
            .Include(s => s.AccessGrants)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<RemoteSession?> GetByCodeWithGrantsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.RemoteSessions
            .Include(s => s.AccessGrants)
            .FirstOrDefaultAsync(s => s.SessionCode == code, cancellationToken);
    }

    public async Task<RemoteSession?> GetActiveSessionByRequesterAsync(Guid requesterId, CancellationToken cancellationToken = default)
    {
        return await _context.RemoteSessions
            .Include(s => s.AccessGrants)
            .Where(s => s.RequesterId == requesterId &&
                        (s.Status == RemoteSessionStatus.Created ||
                         s.Status == RemoteSessionStatus.PendingApproval ||
                         s.Status == RemoteSessionStatus.Active))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RemoteSession>> GetUserHistoryAsync(
        Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        return await _context.RemoteSessions
            .Include(s => s.AccessGrants)
            .Where(s => s.RequesterId == userId || s.SupporterId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RemoteSession>> GetExpiredSessionsAsync(
        DateTimeOffset threshold, CancellationToken cancellationToken = default)
    {
        return await _context.RemoteSessions
            .Where(s => (s.Status == RemoteSessionStatus.Created || s.Status == RemoteSessionStatus.PendingApproval) &&
                        s.ExpiresAt.HasValue && s.ExpiresAt.Value < threshold)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RemoteSession session, CancellationToken cancellationToken = default)
    {
        await _context.RemoteSessions.AddAsync(session, cancellationToken);
    }

    public Task UpdateAsync(RemoteSession session, CancellationToken cancellationToken = default)
    {
        _context.RemoteSessions.Update(session);
        return Task.CompletedTask;
    }
}
