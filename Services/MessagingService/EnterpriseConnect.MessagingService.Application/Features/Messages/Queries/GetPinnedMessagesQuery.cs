using EnterpriseConnect.MessagingService.Application.Features.Messages.DTOs;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Queries;

public record GetPinnedMessagesQuery(Guid ConversationId, Guid UserId) : IRequest<IReadOnlyList<MessageDto>>;
