using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Conversations.DTOs;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Queries;

public class GetConversationMembersQueryHandler : IRequestHandler<GetConversationMembersQuery, IReadOnlyList<ConversationMemberDto>>
{
    private readonly IMessagingUnitOfWork _uow;

    public GetConversationMembersQueryHandler(IMessagingUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<ConversationMemberDto>> Handle(
        GetConversationMembersQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await _uow.Conversations.GetByIdWithMembersAsync(request.ConversationId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy cuộc trò chuyện.");

        var isCallerMember = conversation.Members.Any(m => m.UserId == request.CurrentUserId);
        if (!isCallerMember)
        {
            throw new UnauthorizedAccessException("Bạn không phải thành viên của cuộc trò chuyện này.");
        }

        return conversation.Members
            .OrderByDescending(m => m.MemberRole)
            .ThenBy(m => m.JoinedAt)
            .Select(m => new ConversationMemberDto(
                m.UserId,
                m.MemberRole,
                m.Nickname,
                m.JoinedAt,
                m.LastReadAt,
                m.LastReadMessageId
            ))
            .ToList();
    }
}
