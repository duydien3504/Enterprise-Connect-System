using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Domain.Entities;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Knowledge.Commands.SyncKnowledgeChunks;

public class SyncKnowledgeChunksCommandHandler : IRequestHandler<SyncKnowledgeChunksCommand, SyncKnowledgeResult>
{
    private readonly IMessagingUnitOfWork _uow;

    public SyncKnowledgeChunksCommandHandler(IMessagingUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<SyncKnowledgeResult> Handle(SyncKnowledgeChunksCommand request, CancellationToken cancellationToken)
    {
        var entities = new List<KnowledgeChunk>();

        if (request.Chunks != null && request.Chunks.Count > 0)
        {
            foreach (var item in request.Chunks)
            {
                if (string.IsNullOrWhiteSpace(item.Content))
                {
                    continue;
                }

                entities.Add(new KnowledgeChunk
                {
                    DocumentId = request.DocumentId,
                    DocumentTitle = !string.IsNullOrWhiteSpace(request.DocumentTitle) ? request.DocumentTitle : "Document",
                    Content = item.Content,
                    DepartmentId = item.DepartmentId,
                    RoleId = item.RoleId
                });
            }
        }

        await _uow.KnowledgeChunks.ReplaceChunksAsync(request.DocumentId, entities, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new SyncKnowledgeResult(
            true,
            request.DocumentId,
            entities.Count,
            $"Successfully synchronized {entities.Count} knowledge chunk(s).");
    }
}
