using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.MessagingService.Infrastructure.Services.Ai;

public interface IGroqApiClient
{
    Task<string> CreateChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}

public class GroqApiClient : IGroqApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GroqApiClient> _logger;

    public GroqApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<GroqApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> CreateChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["GroqSettings:ApiKey"];
        var model = _configuration["GroqSettings:Model"] ?? "llama-3.3-70b-versatile";
        var baseUrl = _configuration["GroqSettings:BaseUrl"] ?? "https://api.groq.com/openai/v1";

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GROQ_API_KEY_HERE")
        {
            _logger.LogWarning("Groq API Key chưa được cấu hình. Sử dụng fallback AI response simulation.");
            return SimulatedAiResponse(userPrompt);
        }

        try
        {
            var requestUrl = $"{baseUrl.TrimEnd('/')}/chat/completions";
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.2
            };

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(jsonString);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi Groq API");
            return SimulatedAiResponse(userPrompt);
        }
    }

    private static string SimulatedAiResponse(string userPrompt)
    {
        if (userPrompt.Contains("summarize", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Serialize(new
            {
                summary = "Cuộc trao đổi tập trung vào việc triển khai kiến trúc hệ thống và phân bổ nhiệm vụ phát triển.",
                keyPoints = new[] { "Thống nhất Clean Architecture", "Tích hợp SignalR Realtime", "Sử dụng Groq AI API" },
                decisions = new[] { "Áp dụng cơ chế Hybrid kiểm duyệt tin nhắn", "Dùng PostgreSQL cho lưu trữ" },
                actionItems = new[]
                {
                    new { task = "Hoàn thiện SignalR Hub", assignee = "Backend Team" },
                    new { task = "Thiết kế UI", assignee = "Frontend Team" }
                },
                participants = new[] { "Team Lead", "Developer" }
            });
        }

        if (userPrompt.Contains("toxic", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Serialize(new
            {
                isToxic = false,
                severity = "None",
                reason = "Nội dung giao tiếp bình thường."
            });
        }

        return "Xin chào! Tôi là Trợ lý Doanh nghiệp Enterprise Connect. Tôi có thể hỗ trợ giải đáp các quy trình, chính sách và tài liệu nội bộ.";
    }
}
