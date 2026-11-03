using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.AdminService.Domain.Enums;

namespace EnterpriseConnect.AdminService.Domain.Entities;

public class Document : BaseEntity
{
    public Guid UploaderId { get; set; } // Logical FK to IdentityService.User
    public string Title { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DocumentType FileType { get; set; } = DocumentType.PDF;
    public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;

    public virtual ICollection<DocumentAccess> AccessList { get; set; } = new List<DocumentAccess>();
}
