using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.MessagingService.Infrastructure.Services.Ai;

public class KnowledgeAssistant : IKnowledgeAssistant
{
    private readonly MessagingDbContext _dbContext;
    private readonly IGroqApiClient _groqClient;
    private readonly ILogger<KnowledgeAssistant> _logger;

    public KnowledgeAssistant(MessagingDbContext dbContext, IGroqApiClient groqClient, ILogger<KnowledgeAssistant> logger)
    {
        _dbContext = dbContext;
        _groqClient = groqClient;
        _logger = logger;
    }

    public async Task<KnowledgeAnswerResult> AnswerQuestionAsync(string question, Guid? departmentId, IEnumerable<Guid>? roleIds, CancellationToken cancellationToken = default)
    {
        var roleIdList = roleIds?.ToList() ?? new List<Guid>();

        // 1. Lọc tài liệu theo phân quyền phòng ban và vai trò
        var query = _dbContext.KnowledgeChunks.AsNoTracking()
            .Where(k => (k.DepartmentId == null || k.DepartmentId == departmentId)
                     && (k.RoleId == null || roleIdList.Contains(k.RoleId.Value)));

        if (string.IsNullOrWhiteSpace(question))
        {
            return new KnowledgeAnswerResult(
                "Vui lòng nhập nội dung câu hỏi sau lệnh /a (Ví dụ: /a chính sách bảo mật của công ty là gì?).",
                Array.Empty<string>());
        }

        var punctuation = new[] { ' ', '?', '.', ',', '!', ';', ':', '(', ')', '"', '\'', '/', '\\' };
        var keywords = question.Split(punctuation, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(k => k.Length >= 2)
            .Take(8)
            .ToList();

        if (keywords.Count == 0)
        {
            return new KnowledgeAnswerResult(
                "Rất tiếc, tôi không tìm thấy tài liệu nội bộ nào phù hợp với câu hỏi của bạn hoặc bạn chưa được cấp quyền truy cập tài liệu liên quan.",
                Array.Empty<string>());
        }

        var chunks = await query.ToListAsync(cancellationToken);

        // Lọc top 4 chunks khớp từ khóa (Score > 0) và loại bỏ trùng lặp nội dung
        var relevantChunks = chunks
            .Select(c => new
            {
                Chunk = c,
                Score = keywords.Count(kw => c.Content.Contains(kw, StringComparison.OrdinalIgnoreCase) || c.DocumentTitle.Contains(kw, StringComparison.OrdinalIgnoreCase))
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .GroupBy(x => new { x.Chunk.DocumentId, x.Chunk.Content })
            .Select(g => g.First().Chunk)
            .Take(4)
            .ToList();

        var sources = relevantChunks.Select(c => c.DocumentTitle).Distinct().ToList();

        if (!relevantChunks.Any())
        {
            return new KnowledgeAnswerResult(
                "Rất tiếc, tôi không tìm thấy tài liệu nội bộ nào phù hợp với câu hỏi của bạn hoặc bạn chưa được cấp quyền truy cập tài liệu liên quan.",
                Array.Empty<string>());
        }

        var context = string.Join("\n\n---\n\n", relevantChunks.Select(c => $"[Tài liệu: {c.DocumentTitle}]\n{c.Content}"));

        var systemPrompt = $@"Bạn là Trợ lý Doanh nghiệp Enterprise Connect (AI Knowledge Assistant).
Nhiệm vụ: Trả lời câu hỏi của nhân viên dựa DUY NHẤT trên các thông tin được cung cấp trong tài liệu nội bộ bên dưới.
Nếu tài liệu không chứa thông tin cần thiết, hãy lịch sự trả lời rằng thông tin chưa được cập nhật trong cơ sở dữ liệu nội bộ.
Không tự bịa ra thông tin. Trình bày rõ ràng, thân thiện, chuyên nghiệp.

TÀI LIỆU THAM KHẢO HỢP LỆ:
{context}";

        var userPrompt = $"Câu hỏi của nhân viên: {question}";

        try
        {
            var answer = await _groqClient.CreateChatCompletionAsync(systemPrompt, userPrompt, cancellationToken);
            return new KnowledgeAnswerResult(answer, sources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý RAG Knowledge Assistant");
            return new KnowledgeAnswerResult("Đã xảy ra lỗi khi kết nối tới Trợ lý AI. Vui lòng thử lại sau.", sources);
        }
    }
}
