using EnterpriseConnect.BuildingBlocks.Application.Contracts;
using EnterpriseConnect.MessagingService.Domain.Entities;

namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public interface IKnowledgeChunkRepository : IRepository<KnowledgeChunk>
{
    Task ReplaceChunksAsync(Guid documentId, IEnumerable<KnowledgeChunk> chunks, CancellationToken cancellationToken = default);
}
