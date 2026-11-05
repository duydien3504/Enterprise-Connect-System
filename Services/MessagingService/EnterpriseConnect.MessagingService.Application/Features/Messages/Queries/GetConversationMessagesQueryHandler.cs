using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Messages.DTOs;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Queries;

public class GetConversationMessagesQueryHandler : IRequestHandler<GetConversationMessagesQuery, IReadOnlyList<MessageDto>>
{
    private readonly IMessagingUnitOfWork _uow;

    public GetConversationMessagesQueryHandler(IMessagingUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<MessageDto>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _uow.Conversations.GetByIdWithMembersAsync(request.ConversationId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy cuộc trò chuyện.");

        var isMember = conversation.Members.Any(m => m.UserId == request.UserId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("Bạn không phải thành viên của cuộc trò chuyện này.");
        }

        var messages = await _uow.Messages.GetConversationMessagesAsync(request.ConversationId, request.Skip, request.Take, cancellationToken);

        return messages.Select(m => new MessageDto(
            m.Id,
            m.ConversationId,
            m.SenderId,
            m.Content,
            m.Type,
            m.Status,
            m.CreatedAt,
            m.IsEdited,
            m.IsPinned,
            m.ReplyToId,
            m.Reactions.Select(r => r.Emoji).Distinct().ToList()
        )).ToList();
    }
}
