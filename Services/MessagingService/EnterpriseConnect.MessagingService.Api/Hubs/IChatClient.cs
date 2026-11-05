using EnterpriseConnect.MessagingService.Application.Features.Messages.DTOs;
using EnterpriseConnect.MessagingService.Domain.Enums;

namespace EnterpriseConnect.MessagingService.Api.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(MessageDto message);
    Task MessageStatusChanged(Guid messageId, MessageStatus status);
    Task UserTyping(Guid conversationId, Guid userId, bool isTyping);
    Task MessageReacted(Guid messageId, Guid userId, string emoji, bool isAdded);
    Task MessageEdited(Guid messageId, string newContent, DateTimeOffset editedAt);
    Task MessageDeleted(Guid messageId);
    Task MessagePinned(Guid messageId, bool isPinned, Guid? pinnedBy);
    Task ToxicWarning(string warningMessage);
}
