using EnterpriseConnect.MessagingService.Application.Features.Messages.DTOs;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Queries;

public record GetConversationMessagesQuery(
    Guid ConversationId, 
    Guid UserId, 
    int Skip = 0, 
    int Take = 50
) : IRequest<IReadOnlyList<MessageDto>>;
