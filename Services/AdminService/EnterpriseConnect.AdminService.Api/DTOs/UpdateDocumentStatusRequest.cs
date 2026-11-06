using EnterpriseConnect.AdminService.Domain.Enums;

namespace EnterpriseConnect.AdminService.Api.DTOs;

public class UpdateDocumentStatusRequest
{
    public DocumentStatus Status { get; set; }
}
