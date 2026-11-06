using EnterpriseConnect.AdminService.Domain.Enums;
using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.AdminService.Domain.Entities;

public class Document : BaseEntity
{
    public Guid UploaderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DocumentType FileType { get; set; } = DocumentType.PDF;
    public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;
    public string Version { get; set; } = "1.0";
    public long FileSizeBytes { get; set; }

    public virtual ICollection<DocumentAccess> AccessList { get; set; } = new List<DocumentAccess>();
}
