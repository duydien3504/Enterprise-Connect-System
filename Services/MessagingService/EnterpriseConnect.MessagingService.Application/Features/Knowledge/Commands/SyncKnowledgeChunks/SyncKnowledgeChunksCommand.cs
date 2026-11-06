using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Knowledge.Commands.SyncKnowledgeChunks;

public class SyncKnowledgeChunksCommand : IRequest<SyncKnowledgeResult>
{
    public Guid DocumentId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public List<SyncKnowledgeChunkItem> Chunks { get; set; } = new();
}
