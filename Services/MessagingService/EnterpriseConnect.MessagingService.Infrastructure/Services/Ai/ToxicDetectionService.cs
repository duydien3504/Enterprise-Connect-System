using System.Text.Json;
using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.MessagingService.Infrastructure.Services.Ai;

public class ToxicDetectionService : IToxicDetectionService
{
    private readonly IGroqApiClient _groqClient;
    private readonly ILogger<ToxicDetectionService> _logger;

    public ToxicDetectionService(IGroqApiClient groqClient, ILogger<ToxicDetectionService> logger)
    {
        _groqClient = groqClient;
        _logger = logger;
    }

    public async Task<ToxicCheckResult> AnalyzeContentAsync(string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new ToxicCheckResult(false, "None", "Nội dung rỗng");
        }

        // Bước 1: Kiểm tra từ ngữ xúc phạm trực tiếp bằng bộ lọc nhanh (Rule-based Filter)
        var (isRuleMatched, ruleSeverity, ruleReason) = VietnameseProfanityMatcher.Match(content);
        if (isRuleMatched)
        {
            return new ToxicCheckResult(true, ruleSeverity, ruleReason);
        }

        // Bước 2: Gọi Groq LLM phân tích ngữ cảnh nâng cao nếu không dính từ khóa trực tiếp
        var systemPrompt = @"Bạn là Chuyên gia kiểm duyệt nội dung giao tiếp doanh nghiệp (AI Content Moderator).
Nhiệm vụ: Phân tích tin nhắn xem có chứa ngôn từ tục tĩu, xúc phạm cá nhân, công kích đồng nghiệp hay không.
BẮT BUỘC trả về đúng định dạng JSON không kèm giải thích bên ngoài:
{
  ""isToxic"": true/false,
  ""severity"": ""None"" | ""Low"" | ""Medium"" | ""High"",
  ""reason"": ""Lý do ngắn gọn""
}";

        var userPrompt = $"Tin nhắn cần kiểm tra: \"{content}\"";

        try
        {
            var responseJson = await _groqClient.CreateChatCompletionAsync(systemPrompt, userPrompt, cancellationToken);
            var cleanJson = ExtractJson(responseJson);

            using var doc = JsonDocument.Parse(cleanJson);
            var root = doc.RootElement;
            var isToxic = root.GetProperty("isToxic").GetBoolean();
            var severity = root.GetProperty("severity").GetString() ?? "None";
            var reason = root.GetProperty("reason").GetString() ?? "Hợp lệ";

            return new ToxicCheckResult(isToxic, severity, reason);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể kết nối Groq API cho kiểm duyệt ngữ cảnh, fallback về rule filter.");
            return new ToxicCheckResult(false, "None", "Hợp lệ");
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
