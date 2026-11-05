using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;

public record CreateGroupConversationCommand(Guid OwnerId, string GroupName, List<Guid> MemberIds) : IRequest<Guid>;

public class CreateGroupConversationCommandHandler : IRequestHandler<CreateGroupConversationCommand, Guid>
{
    private readonly IMessagingUnitOfWork _uow;

    public CreateGroupConversationCommandHandler(IMessagingUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateGroupConversationCommand request, CancellationToken cancellationToken)
    {
        var conv = new Conversation
        {
            Type = ConversationType.Group,
            Name = request.GroupName.Trim()
        };

        conv.Members.Add(new ConversationMember { UserId = request.OwnerId, MemberRole = MemberRole.Owner });

        foreach (var memberId in request.MemberIds.Distinct())
        {
            if (memberId != request.OwnerId)
            {
                conv.Members.Add(new ConversationMember { UserId = memberId, MemberRole = MemberRole.Member });
            }
        }

        await _uow.Conversations.AddAsync(conv, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return conv.Id;
    }
}
