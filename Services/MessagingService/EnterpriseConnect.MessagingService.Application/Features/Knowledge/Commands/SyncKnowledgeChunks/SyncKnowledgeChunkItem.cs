namespace EnterpriseConnect.MessagingService.Application.Features.Knowledge.Commands.SyncKnowledgeChunks;

public class SyncKnowledgeChunkItem
{
    public string Content { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? RoleId { get; set; }
}
