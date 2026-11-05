using EnterpriseConnect.MessagingService.Domain.Enums;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.DTOs;

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    MessageType Type,
    MessageStatus Status,
    DateTimeOffset CreatedAt,
    bool IsEdited,
    bool IsPinned,
    Guid? ReplyToId,
    IReadOnlyList<string> Reactions
);
