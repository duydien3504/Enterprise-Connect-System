namespace EnterpriseConnect.AdminService.Application.Common.Models;

public class SyncKnowledgeChunksDto
{
    public Guid DocumentId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public List<KnowledgeChunkItemDto> Chunks { get; set; } = new();
}
