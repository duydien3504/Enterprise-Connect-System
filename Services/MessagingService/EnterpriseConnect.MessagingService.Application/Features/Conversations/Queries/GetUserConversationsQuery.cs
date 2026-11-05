using EnterpriseConnect.MessagingService.Application.Features.Conversations.DTOs;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Queries;

public record GetUserConversationsQuery(Guid UserId) : IRequest<IReadOnlyList<ConversationItemDto>>;
