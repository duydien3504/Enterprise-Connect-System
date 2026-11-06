namespace EnterpriseConnect.AdminService.Application.Common.Models;

public class KnowledgeChunkItemDto
{
    public string Content { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? RoleId { get; set; }
}
