using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Application.Common.Services;
using EnterpriseConnect.AdminService.Domain.Entities;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UpdateDocument;

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, Result<DocumentDto>>
{
    private readonly IAdminUnitOfWork _unitOfWork;
    private readonly IKnowledgeSyncClient? _knowledgeSyncClient;
    private readonly IDocumentChunker? _documentChunker;

    public UpdateDocumentCommandHandler(
        IAdminUnitOfWork unitOfWork,
        IKnowledgeSyncClient? knowledgeSyncClient = null,
        IDocumentChunker? documentChunker = null)
    {
        _unitOfWork = unitOfWork;
        _knowledgeSyncClient = knowledgeSyncClient;
        _documentChunker = documentChunker;
    }

    public async Task<Result<DocumentDto>> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await _unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);
        if (doc == null)
        {
            return Result<DocumentDto>.Failure($"Document with Id '{request.Id}' was not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            doc.Title = request.Title;
        }

        if (request.Description != null)
        {
            doc.Description = request.Description;
        }

        if (!string.IsNullOrWhiteSpace(request.Version))
        {
            doc.Version = request.Version;
        }

        doc.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.RoleIds != null || request.DepartmentIds != null)
        {
            doc.AccessList.Clear();
            if (request.RoleIds != null)
            {
                foreach (var roleId in request.RoleIds)
                {
                    doc.AccessList.Add(new DocumentAccess { DocumentId = doc.Id, RoleId = roleId });
                }
            }
            if (request.DepartmentIds != null)
            {
                foreach (var deptId in request.DepartmentIds)
                {
                    doc.AccessList.Add(new DocumentAccess { DocumentId = doc.Id, DepartmentId = deptId });
                }
            }
        }

        await _unitOfWork.Documents.UpdateAsync(doc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_knowledgeSyncClient != null)
        {
            var syncDto = DocumentKnowledgeSyncBuilder.BuildSyncDto(doc, null, _documentChunker);
            await _knowledgeSyncClient.SyncKnowledgeChunksAsync(syncDto, cancellationToken);
        }

        return Result<DocumentDto>.Success(doc.ToDto());
    }
}
