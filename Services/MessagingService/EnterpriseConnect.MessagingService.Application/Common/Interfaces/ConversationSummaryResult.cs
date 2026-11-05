namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public record ConversationSummaryResult(
    string Summary,
    IReadOnlyList<string> KeyPoints,
    IReadOnlyList<string> Decisions,
    IReadOnlyList<ActionItemDto> ActionItems,
    IReadOnlyList<string> Participants
);
