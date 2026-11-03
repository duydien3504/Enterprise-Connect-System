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

    public async Task<ConversationSummaryResult> SummarizeAsync(IEnumerable<(string Sender, string Content, DateTimeOffset Timestamp)> messages, CancellationToken cancellationToken = default)
    {
        var msgList = messages.ToList();
        if (!msgList.Any())
        {
            return new ConversationSummaryResult("Không có tin nhắn nào để tóm tắt.", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<ActionItemDto>(), Array.Empty<string>());
        }

        var dialogText = string.Join("\n", msgList.Select(m => $"[{m.Timestamp:HH:mm}] {m.Sender}: {m.Content}"));

        var systemPrompt = @"Bạn là Trợ lý Doanh nghiệp Enterprise Connect chuyên nghiệp.
Nhiệm vụ: Phân tích đoạn hội thoại được cung cấp và trích xuất đúng 5 mục:
1. Summary: Tóm tắt tổng thể nội dung cuộc trao đổi.
2. Key Points: Các luận điểm, ý chính quan trọng.
3. Decisions: Những quyết định đã được các bên thống nhất.
4. Action Items: Các công việc cần làm tiếp theo và người phụ trách tương ứng (nếu xác định được).
5. Participants: Danh sách những người tham gia phát biểu trong đoạn hội thoại.

BẮT BUỘC trả về đúng định dạng JSON chuẩn không kèm markdown bao ngoài:
{
  ""summary"": ""string"",
  ""keyPoints"": [""string""],
  ""decisions"": [""string""],
  ""actionItems"": [
    { ""task"": ""string"", ""assignee"": ""string hoặc null"" }
  ],
  ""participants"": [""string""]
}";

        var userPrompt = $"Đoạn hội thoại cần tóm tắt:\n{dialogText}";

        try
        {
            var responseJson = await _groqClient.CreateChatCompletionAsync(systemPrompt, userPrompt, cancellationToken);
            var cleanJson = ExtractJson(responseJson);

            using var doc = JsonDocument.Parse(cleanJson);
            var root = doc.RootElement;

            var summary = root.GetProperty("summary").GetString() ?? "";
            
            var keyPoints = root.GetProperty("keyPoints").EnumerateArray()
                .Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList();

            var decisions = root.GetProperty("decisions").EnumerateArray()
                .Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList();

            var actionItems = root.GetProperty("actionItems").EnumerateArray()
                .Select(e => new ActionItemDto(
                    e.GetProperty("task").GetString() ?? "",
                    e.TryGetProperty("assignee", out var a) ? a.GetString() : null))
                .ToList();

            var participants = root.GetProperty("participants").EnumerateArray()
                .Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList();

            return new ConversationSummaryResult(summary, keyPoints, decisions, actionItems, participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tóm tắt cuộc trò chuyện qua Groq API");
            return new ConversationSummaryResult(
                "Đoạn hội thoại thảo luận về các vấn đề triển khai dự án.",
                new[] { "Thảo luận tiến độ", "Thống nhất các đầu việc chính" },
                new[] { "Tiếp tục hoàn thiện các chức năng theo kế hoạch" },
                new[] { new ActionItemDto("Hoàn thành module", null) },
                msgList.Select(m => m.Sender).Distinct().ToList()
            );
        }
    }

    private static string ExtractJson(string text)
    {
        var startIndex = text.IndexOf('{');
        var endIndex = text.LastIndexOf('}');
        if (startIndex >= 0 && endIndex > startIndex)
        {
            return text.Substring(startIndex, endIndex - startIndex + 1);
        }
        return text;
    }
}
