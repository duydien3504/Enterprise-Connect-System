using EnterpriseConnect.MessagingService.Domain.Entities;

namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public interface IAiSummaryRepository
{
    Task<AiSummary?> GetLatestByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task AddAsync(AiSummary summary, CancellationToken cancellationToken = default);
}
