using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Domain.Enums;
using EnterpriseConnect.MessagingService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.MessagingService.Infrastructure.Persistence.Repositories;

public class MessagingUnitOfWork : IMessagingUnitOfWork
{
    private readonly MessagingDbContext _context;

    public MessagingUnitOfWork(MessagingDbContext context)
    {
        _context = context;
        Conversations = new ConversationRepository(_context);
        Messages = new MessageRepository(_context);
    }

    public IConversationRepository Conversations { get; }
    public IMessageRepository Messages { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

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
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .Include(c => c.Members)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);
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

public class MessageRepository : IMessageRepository
{
    private readonly MessagingDbContext _context;

    public MessageRepository(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Include(m => m.Reactions)
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Message>> GetConversationMessagesAsync(Guid conversationId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Reactions)
            .Include(m => m.Attachments)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        await _context.Messages.AddAsync(message, cancellationToken);
    }

    public Task UpdateAsync(Message message, CancellationToken cancellationToken = default)
    {
        _context.Messages.Update(message);
        return Task.CompletedTask;
    }

    public async Task AddReactionAsync(MessageReaction reaction, CancellationToken cancellationToken = default)
    {
        await _context.MessageReactions.AddAsync(reaction, cancellationToken);
    }

    public async Task RemoveReactionAsync(Guid messageId, Guid userId, string emoji, CancellationToken cancellationToken = default)
    {
        var existing = await _context.MessageReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji, cancellationToken);
        if (existing != null)
        {
            _context.MessageReactions.Remove(existing);
        }
    }
}
