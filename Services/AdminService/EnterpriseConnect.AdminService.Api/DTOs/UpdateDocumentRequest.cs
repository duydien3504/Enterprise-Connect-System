namespace EnterpriseConnect.AdminService.Api.DTOs;

public class UpdateDocumentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public List<Guid>? RoleIds { get; set; }
    public List<Guid>? DepartmentIds { get; set; }
}
