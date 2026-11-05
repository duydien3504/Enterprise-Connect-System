using EnterpriseConnect.MessagingService.Domain.Enums;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.DTOs;

public record ConversationItemDto(
    Guid Id,
    ConversationType Type,
    string? Name,
    string? AvatarUrl,
    int MemberCount,
    DateTimeOffset? LastMessageAt,
    DateTimeOffset CreatedAt
);
