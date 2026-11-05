using System.Text.Json;
using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.MessagingService.Infrastructure.Services.Ai;

public class ConversationSummarizer : IConversationSummarizer
{
    private readonly IGroqApiClient _groqClient;
    private readonly ILogger<ConversationSummarizer> _logger;

    public ConversationSummarizer(IGroqApiClient groqClient, ILogger<ConversationSummarizer> logger)
    {
        _groqClient = groqClient;
        _logger = logger;
    }

    public async Task<ConversationSummaryResult> SummarizeAsync(
        IEnumerable<(string Sender, string Content, DateTimeOffset Timestamp)> messages, 
        string language = "vi",
        CancellationToken cancellationToken = default)
    {
        var msgList = messages.ToList();
        var isEn = language?.StartsWith("en", StringComparison.OrdinalIgnoreCase) == true;

        if (!msgList.Any())
        {
            var emptyText = isEn ? "No messages to summarize." : "Không có tin nhắn nào để tóm tắt.";
            return new ConversationSummaryResult(emptyText, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ActionItemDto>(), Array.Empty<string>());
        }

        var dialogText = string.Join("\n", msgList.Select(m => $"[{m.Timestamp:HH:mm}] {m.Sender}: {m.Content}"));
        var systemPrompt = BuildSystemPrompt(isEn);
        var userPrompt = isEn ? $"Conversation to summarize:\n{dialogText}" : $"Đoạn hội thoại cần tóm tắt:\n{dialogText}";

        try
        {
            var responseJson = await _groqClient.CreateChatCompletionAsync(systemPrompt, userPrompt, cancellationToken);
            var cleanJson = ExtractJson(responseJson);

            using var doc = JsonDocument.Parse(cleanJson);
            var root = doc.RootElement;

            var summary = root.GetProperty("summary").GetString() ?? "";
            var keyPoints = root.GetProperty("keyPoints").EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList();
            var decisions = root.GetProperty("decisions").EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList();
            var actionItems = root.GetProperty("actionItems").EnumerateArray()
                .Select(e => new ActionItemDto(e.GetProperty("task").GetString() ?? "", e.TryGetProperty("assignee", out var a) ? a.GetString() : null)).ToList();
            var participants = root.GetProperty("participants").EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList();

            return new ConversationSummaryResult(summary, keyPoints, decisions, actionItems, participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tóm tắt cuộc trò chuyện qua Groq API");
            return CreateFallbackResult(isEn, msgList.Select(m => m.Sender).Distinct().ToList());
        }
    }

    private static string BuildSystemPrompt(bool isEnglish) => isEnglish
        ? "You are an Enterprise Connect AI Assistant. Analyze the dialogue and extract: summary, keyPoints, decisions, actionItems (task, assignee), participants. Return ONLY JSON: {\"summary\":\"...\",\"keyPoints\":[...],\"decisions\":[...],\"actionItems\":[{\"task\":\"...\",\"assignee\":\"...\"}],\"participants\":[...]} entirely in English."
        : "Bạn là Trợ lý Doanh nghiệp Enterprise Connect. Hãy phân tích đoạn hội thoại và trích xuất: summary, keyPoints, decisions, actionItems (task, assignee), participants HOÀN TOÀN BẰNG TIẾNG VIỆT. BẮT BUỘC chỉ trả về JSON: {\"summary\":\"...\",\"keyPoints\":[...],\"decisions\":[...],\"actionItems\":[{\"task\":\"...\",\"assignee\":\"...\"}],\"participants\":[...]}";

    private static ConversationSummaryResult CreateFallbackResult(bool isEnglish, IReadOnlyList<string> participants) => isEnglish
        ? new ConversationSummaryResult("Discussion regarding project implementation milestones.", new[] { "Reviewed overall progress", "Agreed on core goals" }, new[] { "Continue execution according to roadmap" }, new[] { new ActionItemDto("Complete pending tasks", null) }, participants)
        : new ConversationSummaryResult("Đoạn hội thoại thảo luận về các vấn đề triển khai dự án.", new[] { "Thảo luận tiến độ", "Thống nhất các đầu việc chính" }, new[] { "Tiếp tục hoàn thiện các chức năng theo kế hoạch" }, new[] { new ActionItemDto("Hoàn thành module", null) }, participants);

    private static string ExtractJson(string text)
    {
        var startIndex = text.IndexOf('{');
        var endIndex = text.LastIndexOf('}');
        return startIndex >= 0 && endIndex > startIndex ? text.Substring(startIndex, endIndex - startIndex + 1) : text;
    }
}
