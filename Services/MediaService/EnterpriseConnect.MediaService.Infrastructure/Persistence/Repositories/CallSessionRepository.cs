using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Domain.Entities;
using EnterpriseConnect.MediaService.Domain.Enums;
using EnterpriseConnect.MediaService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.MediaService.Infrastructure.Persistence.Repositories;

public class CallSessionRepository : ICallSessionRepository
{
    private readonly MediaDbContext _context;

    public CallSessionRepository(MediaDbContext context)
    {
        _context = context;
    }

    public async Task<CallSession?> GetByIdWithParticipantsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.CallSessions
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CallSession?> GetActiveCallByConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CallSessions
            .Include(c => c.Participants)
            .Where(c => c.ConversationId == conversationId &&
                        (c.Status == CallStatus.Initiated ||
                         c.Status == CallStatus.Ringing ||
                         c.Status == CallStatus.Ongoing))
            .OrderByDescending(c => c.StartTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CallSession>> GetUserCallHistoryAsync(
        Guid userId,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        return await _context.CallSessions
            .Include(c => c.Participants)
            .Where(c => c.InitiatorId == userId || c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.StartTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        CallSession callSession,
        CancellationToken cancellationToken = default)
    {
        await _context.CallSessions.AddAsync(callSession, cancellationToken);
    }

    public Task UpdateAsync(
        CallSession callSession,
        CancellationToken cancellationToken = default)
    {
        _context.CallSessions.Update(callSession);
        return Task.CompletedTask;
    }
}
