using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;

public record TogglePinMessageCommand(Guid MessageId, Guid UserId) : IRequest<bool>;

public class TogglePinMessageCommandHandler : IRequestHandler<TogglePinMessageCommand, bool>
{
    private readonly IMessagingUnitOfWork _uow;

    public TogglePinMessageCommandHandler(IMessagingUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(TogglePinMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _uow.Messages.GetByIdAsync(request.MessageId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy tin nhắn.");

        message.IsPinned = !message.IsPinned;
        message.PinnedAt = message.IsPinned ? DateTimeOffset.UtcNow : null;
        message.PinnedByUserId = message.IsPinned ? request.UserId : null;

        await _uow.Messages.UpdateAsync(message, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return message.IsPinned;
    }
}
