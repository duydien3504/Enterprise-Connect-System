using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;

public record CreateGroupConversationCommand(Guid OwnerId, string GroupName, List<Guid> MemberIds) : IRequest<Guid>;
