using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;

public record CreateDirectConversationCommand(Guid CurrentUserId, Guid TargetUserId) : IRequest<Guid>;
