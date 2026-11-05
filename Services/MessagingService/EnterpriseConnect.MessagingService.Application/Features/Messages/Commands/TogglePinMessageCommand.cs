using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;

public record TogglePinMessageCommand(Guid MessageId, Guid UserId) : IRequest<bool>;
