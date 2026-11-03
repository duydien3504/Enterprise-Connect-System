namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public record ToxicCheckResult(bool IsToxic, string Severity, string Reason);

public interface IToxicDetectionService
{
    Task<ToxicCheckResult> AnalyzeContentAsync(string content, CancellationToken cancellationToken = default);
}

public record ConversationSummaryResult(
    string Summary,
    IReadOnlyList<string> KeyPoints,
    IReadOnlyList<string> Decisions,
    IReadOnlyList<ActionItemDto> ActionItems,
    IReadOnlyList<string> Participants
);

public record ActionItemDto(string Task, string? Assignee);

public interface IConversationSummarizer
{
    Task<ConversationSummaryResult> SummarizeAsync(IEnumerable<(string Sender, string Content, DateTimeOffset Timestamp)> messages, CancellationToken cancellationToken = default);
}

public record KnowledgeAnswerResult(string Answer, IReadOnlyList<string> Sources);

public interface IKnowledgeAssistant
{
    Task<KnowledgeAnswerResult> AnswerQuestionAsync(string question, Guid? departmentId, IEnumerable<Guid>? roleIds, CancellationToken cancellationToken = default);
}
