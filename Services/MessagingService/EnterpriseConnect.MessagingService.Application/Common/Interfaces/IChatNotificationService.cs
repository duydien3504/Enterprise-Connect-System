namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public interface IChatNotificationService
{
    Task NotifyMessageReceivedAsync(Guid conversationId, object messageDto, CancellationToken cancellationToken = default);
    Task NotifyMessagePinnedAsync(Guid conversationId, Guid messageId, bool isPinned, Guid? pinnedBy, CancellationToken cancellationToken = default);
    Task NotifyMessageReactedAsync(Guid conversationId, Guid messageId, Guid userId, string emoji, bool isAdded, CancellationToken cancellationToken = default);
    Task NotifyUserTypingAsync(Guid conversationId, Guid userId, bool isTyping, CancellationToken cancellationToken = default);
}
