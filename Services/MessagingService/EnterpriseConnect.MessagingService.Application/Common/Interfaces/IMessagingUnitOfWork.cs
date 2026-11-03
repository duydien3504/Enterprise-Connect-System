using EnterpriseConnect.MessagingService.Domain.Entities;

namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public interface IMessagingUnitOfWork
{
    IConversationRepository Conversations { get; }
    IMessageRepository Messages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IConversationRepository
{
    Task<Conversation?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Conversation?> FindDirectConversationAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);
}

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Message>> GetConversationMessagesAsync(Guid conversationId, int skip, int take, CancellationToken cancellationToken = default);
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
    Task UpdateAsync(Message message, CancellationToken cancellationToken = default);
    Task AddReactionAsync(MessageReaction reaction, CancellationToken cancellationToken = default);
    Task RemoveReactionAsync(Guid messageId, Guid userId, string emoji, CancellationToken cancellationToken = default);
}
