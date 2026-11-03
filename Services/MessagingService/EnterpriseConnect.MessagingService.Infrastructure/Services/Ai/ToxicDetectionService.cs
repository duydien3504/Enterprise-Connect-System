using System.Text.Json;
using System.Text.RegularExpressions;
using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.MessagingService.Infrastructure.Services.Ai;

public class ToxicDetectionService : IToxicDetectionService
{
    private readonly IGroqApiClient _groqClient;
    private readonly ILogger<ToxicDetectionService> _logger;

    // Danh sách regex từ ngữ thô tục / xúc phạm cấm tuyệt đối (Fast pre-check)
    private static readonly Regex ProfanityRegex = new(
        @"\b(đ[iị]t|l[ồo]n|c[ặa]c|bu[ồo]i|đ[eé]o|m[ẹe]\s*m[àa]y|ch[óo]\s*ch[ếe]t|th[ằa]ng\s*ngu)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

        // Bước 1: Fast Regex Pre-check
        var isRegexMatched = ProfanityRegex.IsMatch(content);

        // Bước 2: Gọi Groq LLM phân tích ngữ cảnh (phân biệt chửi thề xúc phạm cá nhân vs trích dẫn/thảo luận chuyên môn)
        var systemPrompt = @"Bạn là Chuyên gia kiểm duyệt nội dung giao tiếp doanh nghiệp (AI Content Moderator).
Nhiệm vụ: Phân tích tin nhắn xem có chứa ngôn từ tục tĩu, xúc phạm cá nhân, công kích đồng nghiệp hay không.
ĐẶC BIỆT: Hãy phân biệt giữa việc chửi tục xúc phạm trực tiếp người khác VỚI trường hợp người dùng trích dẫn câu chữ, thảo luận lỗi phần mềm/kỹ thuật hoặc nhắc lại câu nói mà không có ý đồ xúc phạm.
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
            var reason = root.GetProperty("reason").GetString() ?? "";

            // Nếu regex bắt được nhưng LLM bảo không, ưu tiên cảnh báo nếu mức độ thô tục rõ ràng
            if (isRegexMatched && !isToxic)
            {
                isToxic = true;
                severity = "Low";
                reason = "Phát hiện từ ngữ nhạy cảm trong giao tiếp doanh nghiệp.";
            }

            return new ToxicCheckResult(isToxic, severity, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi phân tích Toxic bằng Groq API");
            if (isRegexMatched)
            {
                return new ToxicCheckResult(true, "Medium", "Phát hiện từ ngữ thô tục qua hệ thống lọc nhanh.");
            }
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
