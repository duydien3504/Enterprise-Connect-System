using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Conversations.DTOs;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Queries;

public class GetUserConversationsQueryHandler : IRequestHandler<GetUserConversationsQuery, IReadOnlyList<ConversationItemDto>>
{
    private readonly IMessagingUnitOfWork _uow;

    public GetUserConversationsQueryHandler(IMessagingUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<ConversationItemDto>> Handle(GetUserConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _uow.Conversations.GetUserConversationsAsync(request.UserId, cancellationToken);

        return conversations.Select(c => new ConversationItemDto(
            c.Id,
            c.Type,
            c.Name,
            c.AvatarUrl,
            c.Members.Count,
            c.LastMessageAt,
            c.CreatedAt
        )).ToList();
    }
}
