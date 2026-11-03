using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.MessagingService.Domain.Entities;

public class KnowledgeChunk : BaseEntity
{
    public Guid DocumentId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? RoleId { get; set; }
    public float[]? Embedding { get; set; }
}
