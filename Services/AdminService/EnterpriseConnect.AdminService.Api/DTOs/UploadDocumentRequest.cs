using EnterpriseConnect.AdminService.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EnterpriseConnect.AdminService.Api.DTOs;

public class UploadDocumentRequest
{
    public IFormFile? File { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? FileUrl { get; set; }
    public DocumentType? FileType { get; set; }
    public string Version { get; set; } = "1.0";
    public long? FileSizeBytes { get; set; }
    public List<Guid>? RoleIds { get; set; }
    public List<Guid>? DepartmentIds { get; set; }
}
