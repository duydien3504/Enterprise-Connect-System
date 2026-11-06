using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Infrastructure.Persistence.Context;

namespace EnterpriseConnect.MessagingService.Infrastructure.Persistence.Repositories;

public class MessagingUnitOfWork : IMessagingUnitOfWork
{
    private readonly MessagingDbContext _context;

    public MessagingUnitOfWork(MessagingDbContext context)
    {
        _context = context;
        Conversations = new ConversationRepository(_context);
        Messages = new MessageRepository(_context);
        AiSummaries = new AiSummaryRepository(_context);
        KnowledgeChunks = new KnowledgeChunkRepository(_context);
    }

    public IConversationRepository Conversations { get; }
    public IMessageRepository Messages { get; }
    public IAiSummaryRepository AiSummaries { get; }
    public IKnowledgeChunkRepository KnowledgeChunks { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
