using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentCommand : IRequest<Result<DocumentDto>>
{
    public Guid UploaderId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? FileUrl { get; set; }
    public Stream? FileStream { get; set; }
    public string? FileName { get; set; }
    public DocumentType? FileType { get; set; }
    public string Version { get; set; } = "1.0";
    public long? FileSizeBytes { get; set; }
    public List<Guid>? RoleIds { get; set; }
    public List<Guid>? DepartmentIds { get; set; }
}
