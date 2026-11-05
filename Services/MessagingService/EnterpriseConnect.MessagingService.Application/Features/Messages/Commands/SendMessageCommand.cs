using EnterpriseConnect.MessagingService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;

public record SendMessageCommand(
    Guid ConversationId,
    Guid SenderId,
    string Content,
    MessageType Type = MessageType.Text,
    Guid? ReplyToId = null
) : IRequest<SendMessageResult>;
