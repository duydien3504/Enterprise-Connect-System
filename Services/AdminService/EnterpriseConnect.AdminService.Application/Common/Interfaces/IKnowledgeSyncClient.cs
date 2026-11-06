using EnterpriseConnect.AdminService.Application.Common.Models;

namespace EnterpriseConnect.AdminService.Application.Common.Interfaces;

public interface IKnowledgeSyncClient
{
    Task SyncKnowledgeChunksAsync(SyncKnowledgeChunksDto dto, CancellationToken cancellationToken = default);
}
