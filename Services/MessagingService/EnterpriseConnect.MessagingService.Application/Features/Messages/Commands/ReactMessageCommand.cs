using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;

public record ReactMessageCommand(Guid MessageId, Guid UserId, string Emoji) : IRequest<bool>;
