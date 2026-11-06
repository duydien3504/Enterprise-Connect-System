using EnterpriseConnect.BuildingBlocks.Infrastructure.Persistence;
using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.MessagingService.Infrastructure.Persistence.Repositories;

public class KnowledgeChunkRepository : EfRepository<KnowledgeChunk>, IKnowledgeChunkRepository
{
    private readonly MessagingDbContext _messagingDbContext;

    public KnowledgeChunkRepository(MessagingDbContext context) : base(context)
    {
        _messagingDbContext = context;
    }

    public async Task ReplaceChunksAsync(Guid documentId, IEnumerable<KnowledgeChunk> chunks, CancellationToken cancellationToken = default)
    {
        var existing = await _messagingDbContext.KnowledgeChunks
            .Where(k => k.DocumentId == documentId)
            .ToListAsync(cancellationToken);

        if (existing.Count > 0)
        {
            _messagingDbContext.KnowledgeChunks.RemoveRange(existing);
        }

        var chunkList = chunks.ToList();
        if (chunkList.Count > 0)
        {
            await _messagingDbContext.KnowledgeChunks.AddRangeAsync(chunkList, cancellationToken);
        }
    }
}
