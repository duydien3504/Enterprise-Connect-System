using EnterpriseConnect.MessagingService.Domain.Entities;

namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Message>> GetConversationMessagesAsync(Guid conversationId, int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Message>> GetPinnedMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
    Task UpdateAsync(Message message, CancellationToken cancellationToken = default);
    Task AddReactionAsync(MessageReaction reaction, CancellationToken cancellationToken = default);
    Task RemoveReactionAsync(Guid messageId, Guid userId, string emoji, CancellationToken cancellationToken = default);
}
