namespace EnterpriseConnect.MessagingService.Application.Features.Knowledge.Commands.SyncKnowledgeChunks;

public record SyncKnowledgeResult(bool Success, Guid DocumentId, int ChunksCount, string Message);
