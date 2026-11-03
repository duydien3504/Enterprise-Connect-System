using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Domain.Entities;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;

public record ReactMessageCommand(Guid MessageId, Guid UserId, string Emoji) : IRequest<bool>;

public class ReactMessageCommandHandler : IRequestHandler<ReactMessageCommand, bool>
{
    private readonly IMessagingUnitOfWork _uow;

    public ReactMessageCommandHandler(IMessagingUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(ReactMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _uow.Messages.GetByIdAsync(request.MessageId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy tin nhắn.");

        var existingReaction = message.Reactions.FirstOrDefault(r => r.UserId == request.UserId && r.Emoji == request.Emoji);
        if (existingReaction != null)
        {
            // Toggle gỡ reaction
            await _uow.Messages.RemoveReactionAsync(request.MessageId, request.UserId, request.Emoji, cancellationToken);
        }
        else
        {
            // Thêm reaction
            await _uow.Messages.AddReactionAsync(new MessageReaction
            {
                MessageId = request.MessageId,
                UserId = request.UserId,
                Emoji = request.Emoji
            }, cancellationToken);
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

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
