using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.MessagingService.Infrastructure.Persistence.Repositories;

public class AiSummaryRepository : IAiSummaryRepository
{
    private readonly MessagingDbContext _context;

    public AiSummaryRepository(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<AiSummary?> GetLatestByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _context.AiSummaries
            .Where(s => s.ConversationId == conversationId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(AiSummary summary, CancellationToken cancellationToken = default)
    {
        await _context.AiSummaries.AddAsync(summary, cancellationToken);
    }
}
