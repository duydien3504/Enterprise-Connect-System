using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;

public record SummarizeConversationCommand(
    Guid ConversationId, 
    Guid UserId, 
    int MessageCount = 50,
    string Language = "vi"
) : IRequest<ConversationSummaryResult>;
