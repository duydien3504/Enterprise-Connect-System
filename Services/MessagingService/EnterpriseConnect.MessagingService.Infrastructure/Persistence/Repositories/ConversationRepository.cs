using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Domain.Enums;
using EnterpriseConnect.MessagingService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.MessagingService.Infrastructure.Persistence.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly MessagingDbContext _context;

    public ConversationRepository(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Conversation?> FindDirectConversationAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.Type == ConversationType.Direct)
            .Where(c => c.Members.Any(m => m.UserId == userA) && c.Members.Any(m => m.UserId == userB))
            .Include(c => c.Members)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rawConversations = await _context.Conversations
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .Include(c => c.Members)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);

        // Deduplicate any duplicate direct conversations with the same partner
        var distinctList = new List<Conversation>();
        var seenPartners = new HashSet<Guid>();

        foreach (var conv in rawConversations)
        {
            if (conv.Type == ConversationType.Direct)
            {
                var otherMember = conv.Members.FirstOrDefault(m => m.UserId != userId);
                if (otherMember != null)
                {
                    if (seenPartners.Contains(otherMember.UserId))
                    {
                        continue; // Skip duplicate direct conversation with the same partner
                    }
                    seenPartners.Add(otherMember.UserId);
                }
            }
            distinctList.Add(conv);
        }

        return distinctList;
    }

    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _context.Conversations.AddAsync(conversation, cancellationToken);
    }

    public Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        _context.Conversations.Update(conversation);
        return Task.CompletedTask;
    }
}
