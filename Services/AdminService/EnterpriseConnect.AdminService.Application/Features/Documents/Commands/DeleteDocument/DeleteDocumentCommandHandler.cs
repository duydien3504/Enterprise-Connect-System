using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.DeleteDocument;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result<bool>>
{
    private readonly IAdminUnitOfWork _unitOfWork;
    private readonly IKnowledgeSyncClient? _knowledgeSyncClient;

    public DeleteDocumentCommandHandler(
        IAdminUnitOfWork unitOfWork,
        IKnowledgeSyncClient? knowledgeSyncClient = null)
    {
        _unitOfWork = unitOfWork;
        _knowledgeSyncClient = knowledgeSyncClient;
    }

    public async Task<Result<bool>> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await _unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);
        if (doc == null)
        {
            return Result<bool>.Failure($"Document with Id '{request.Id}' was not found.");
        }

        await _unitOfWork.Documents.DeleteAsync(doc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_knowledgeSyncClient != null)
        {
            var purgeDto = new SyncKnowledgeChunksDto
            {
                DocumentId = doc.Id,
                DocumentTitle = doc.Title,
                Chunks = new List<KnowledgeChunkItemDto>()
            };
            await _knowledgeSyncClient.SyncKnowledgeChunksAsync(purgeDto, cancellationToken);
        }

        return Result<bool>.Success(true);
    }
}
