using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Application.Common.Services;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UpdateDocumentStatus;

public class UpdateDocumentStatusCommandHandler : IRequestHandler<UpdateDocumentStatusCommand, Result<DocumentDto>>
{
    private readonly IAdminUnitOfWork _unitOfWork;
    private readonly IKnowledgeSyncClient? _knowledgeSyncClient;
    private readonly IDocumentChunker? _documentChunker;

    public UpdateDocumentStatusCommandHandler(
        IAdminUnitOfWork unitOfWork,
        IKnowledgeSyncClient? knowledgeSyncClient = null,
        IDocumentChunker? documentChunker = null)
    {
        _unitOfWork = unitOfWork;
        _knowledgeSyncClient = knowledgeSyncClient;
        _documentChunker = documentChunker;
    }

    public async Task<Result<DocumentDto>> Handle(UpdateDocumentStatusCommand request, CancellationToken cancellationToken)
    {
        var doc = await _unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);
        if (doc == null)
        {
            return Result<DocumentDto>.Failure($"Document with Id '{request.Id}' was not found.");
        }

        doc.Status = request.Status;
        doc.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.Documents.UpdateAsync(doc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_knowledgeSyncClient != null)
        {
            var syncDto = request.Status == DocumentStatus.Indexed
                ? DocumentKnowledgeSyncBuilder.BuildSyncDto(doc, null, _documentChunker)
                : new SyncKnowledgeChunksDto
                {
                    DocumentId = doc.Id,
                    DocumentTitle = doc.Title,
                    Chunks = new List<KnowledgeChunkItemDto>()
                };
            await _knowledgeSyncClient.SyncKnowledgeChunksAsync(syncDto, cancellationToken);
        }

        return Result<DocumentDto>.Success(doc.ToDto());
    }
}
