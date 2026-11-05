using EnterpriseConnect.MessagingService.Domain.Entities;

namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Conversation?> FindDirectConversationAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);
}
