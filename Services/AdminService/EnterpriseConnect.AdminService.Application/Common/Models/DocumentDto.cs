using EnterpriseConnect.AdminService.Domain.Enums;

namespace EnterpriseConnect.AdminService.Application.Common.Models;

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid UploaderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DocumentType FileType { get; set; }
    public DocumentStatus Status { get; set; }
    public string Version { get; set; } = "1.0";
    public long FileSizeBytes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<DocumentAccessDto> AccessList { get; set; } = new();
}
