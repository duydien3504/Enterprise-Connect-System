using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Application.Common.Services;
using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<DocumentDto>>
{
    private readonly IAdminUnitOfWork _unitOfWork;
    private readonly ICloudinaryStorageService _cloudinaryStorage;
    private readonly IKnowledgeSyncClient? _knowledgeSyncClient;
    private readonly IDocumentChunker? _documentChunker;

    public UploadDocumentCommandHandler(
        IAdminUnitOfWork unitOfWork,
        ICloudinaryStorageService cloudinaryStorage,
        IKnowledgeSyncClient? knowledgeSyncClient = null,
        IDocumentChunker? documentChunker = null)
    {
        _unitOfWork = unitOfWork;
        _cloudinaryStorage = cloudinaryStorage;
        _knowledgeSyncClient = knowledgeSyncClient;
        _documentChunker = documentChunker;
    }

    public async Task<Result<DocumentDto>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        string fileUrl;
        long fileSizeBytes = request.FileSizeBytes ?? 0;
        var fileType = request.FileType ?? DocumentType.PDF;
        string? rawText = null;

        if (request.FileStream != null)
        {
            var fileName = request.FileName ?? $"doc_{Guid.NewGuid():N}.pdf";
            fileType = DetectFileType(fileName, request.FileType);
            if (fileType == DocumentType.TXT)
            {
                using var reader = new StreamReader(request.FileStream, leaveOpen: true);
                rawText = await reader.ReadToEndAsync(cancellationToken);
                if (request.FileStream.CanSeek) request.FileStream.Position = 0;
            }
            fileUrl = await _cloudinaryStorage.UploadRawFileAsync(request.FileStream, fileName, "documents", cancellationToken);
            fileSizeBytes = fileSizeBytes > 0 ? fileSizeBytes : request.FileStream.Length;
        }
        else
        {
            fileUrl = request.FileUrl ?? string.Empty;
        }

        var title = !string.IsNullOrWhiteSpace(request.Title) 
            ? request.Title 
            : Path.GetFileNameWithoutExtension(request.FileName ?? "Tai_Lieu_Noi_Bo");

        var doc = new Document
        {
            UploaderId = request.UploaderId,
            Title = title,
            Description = request.Description,
            FileUrl = fileUrl,
            FileType = fileType,
            Status = DocumentStatus.Uploaded,
            Version = request.Version,
            FileSizeBytes = fileSizeBytes
        };

        if (request.RoleIds != null)
        {
            foreach (var roleId in request.RoleIds)
                doc.AccessList.Add(new DocumentAccess { RoleId = roleId });
        }

        if (request.DepartmentIds != null)
        {
            foreach (var deptId in request.DepartmentIds)
                doc.AccessList.Add(new DocumentAccess { DepartmentId = deptId });
        }

        await _unitOfWork.Documents.AddAsync(doc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_knowledgeSyncClient != null)
        {
            var syncDto = DocumentKnowledgeSyncBuilder.BuildSyncDto(doc, rawText, _documentChunker);
            await _knowledgeSyncClient.SyncKnowledgeChunksAsync(syncDto, cancellationToken);
            doc.Status = DocumentStatus.Indexed;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<DocumentDto>.Success(doc.ToDto());
    }

    private static DocumentType DetectFileType(string fileName, DocumentType? requestedType)
    {
        if (requestedType.HasValue) return requestedType.Value;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".doc" or ".docx" => DocumentType.DOCX,
            ".xls" or ".xlsx" or ".csv" => DocumentType.Excel,
            ".txt" or ".md" => DocumentType.TXT,
            ".pdf" => DocumentType.PDF,
            _ => DocumentType.Other
        };
    }
}
