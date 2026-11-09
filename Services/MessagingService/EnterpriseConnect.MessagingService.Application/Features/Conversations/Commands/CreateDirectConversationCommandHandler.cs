using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;

public class CreateDirectConversationCommandHandler : IRequestHandler<CreateDirectConversationCommand, Guid>
{
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IMessagingUnitOfWork _uow;

    public CreateDirectConversationCommandHandler(IMessagingUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateDirectConversationCommand request, CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == request.TargetUserId)
        {
            throw new ArgumentException("Không thể tạo cuộc trò chuyện trực tiếp với chính mình.");
        }

        // Fast-path check without acquiring semaphore
        var existing = await _uow.Conversations.FindDirectConversationAsync(request.CurrentUserId, request.TargetUserId, cancellationToken);
        if (existing != null)
        {
            return existing.Id;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Re-check after acquiring semaphore to prevent race conditions
            existing = await _uow.Conversations.FindDirectConversationAsync(request.CurrentUserId, request.TargetUserId, cancellationToken);
            if (existing != null)
            {
                return existing.Id;
            }

            var conv = new Conversation
            {
                Type = ConversationType.Direct
            };

            conv.Members.Add(new ConversationMember { UserId = request.CurrentUserId, MemberRole = MemberRole.Member });
            conv.Members.Add(new ConversationMember { UserId = request.TargetUserId, MemberRole = MemberRole.Member });

            await _uow.Conversations.AddAsync(conv, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return conv.Id;
        }
        finally
        {
            _lock.Release();
        }
    }
}
