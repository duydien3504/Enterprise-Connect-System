using EnterpriseConnect.MessagingService.Application.Features.Conversations.DTOs;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Queries;

public record GetConversationMembersQuery(
    Guid ConversationId,
    Guid CurrentUserId
) : IRequest<IReadOnlyList<ConversationMemberDto>>;
