namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public interface IConversationSummarizer
{
    Task<ConversationSummaryResult> SummarizeAsync(
        IEnumerable<(string Sender, string Content, DateTimeOffset Timestamp)> messages, 
        string language = "vi",
        CancellationToken cancellationToken = default);
}
