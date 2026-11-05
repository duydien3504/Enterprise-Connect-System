using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Domain.Entities;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;

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
