using EnterpriseConnect.MessagingService.Domain.Enums;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.DTOs;

public record ConversationMemberDto(
    Guid UserId,
    MemberRole MemberRole,
    string? Nickname,
    DateTimeOffset JoinedAt,
    DateTimeOffset? LastReadAt,
    Guid? LastReadMessageId
);
