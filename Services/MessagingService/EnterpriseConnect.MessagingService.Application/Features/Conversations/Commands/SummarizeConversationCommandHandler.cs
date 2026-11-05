using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Domain.Entities;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;

public class SummarizeConversationCommandHandler : IRequestHandler<SummarizeConversationCommand, ConversationSummaryResult>
{
    private readonly IMessagingUnitOfWork _uow;
    private readonly IConversationSummarizer _summarizer;

    public SummarizeConversationCommandHandler(IMessagingUnitOfWork uow, IConversationSummarizer summarizer)
    {
        _uow = uow;
        _summarizer = summarizer;
    }

    public async Task<ConversationSummaryResult> Handle(SummarizeConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _uow.Conversations.GetByIdWithMembersAsync(request.ConversationId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy cuộc trò chuyện.");

        var messages = await _uow.Messages.GetConversationMessagesAsync(request.ConversationId, 0, request.MessageCount, cancellationToken);
        var orderedMessages = messages.OrderBy(m => m.CreatedAt).ToList();

        var messageTuples = orderedMessages.Select(m => (
            Sender: m.SenderId == Guid.Empty ? "AI Bot" : $"User_{m.SenderId.ToString().Substring(0, 5)}",
            Content: m.Content,
            Timestamp: m.CreatedAt
        ));

        var result = await _summarizer.SummarizeAsync(messageTuples, request.Language, cancellationToken);

        // Lưu vào AiSummary trong Database
        var summaryEntity = new AiSummary
        {
            ConversationId = request.ConversationId,
            RequestedByUserId = request.UserId,
            SummaryText = result.Summary,
            KeyPoints = string.Join("\n", result.KeyPoints),
            Decisions = string.Join("\n", result.Decisions),
            ActionItems = string.Join("\n", result.ActionItems.Select(a => $"{a.Task} - {a.Assignee ?? "Chưa rõ"}")),
            Participants = string.Join(", ", result.Participants)
        };

        await _uow.AiSummaries.AddAsync(summaryEntity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return result;
    }
}
