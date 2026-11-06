using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Infrastructure.Persistence.Context;
using EnterpriseConnect.MessagingService.Infrastructure.Services.Ai;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EnterpriseConnect.MessagingService.UnitTests;

public class KnowledgeAssistantTests
{
    private readonly Mock<IGroqApiClient> _groqMock = new();
    private readonly Mock<ILogger<KnowledgeAssistant>> _loggerMock = new();

    private MessagingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new MessagingDbContext(options);
    }

    [Fact]
    public async Task AnswerQuestionAsync_EmptyQuestion_ReturnsPromptMessage()
    {
        using var db = CreateDbContext();
        var assistant = new KnowledgeAssistant(db, _groqMock.Object, _loggerMock.Object);

        var result = await assistant.AnswerQuestionAsync("   ", null, null);

        result.Answer.Should().Contain("Vui lòng nhập nội dung câu hỏi sau lệnh /a");
        _groqMock.Verify(g => g.CreateChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AnswerQuestionAsync_NoMatchingKeywords_ReturnsNotFound()
    {
        using var db = CreateDbContext();
        db.KnowledgeChunks.Add(new KnowledgeChunk { DocumentId = Guid.NewGuid(), DocumentTitle = "HR Policy", Content = "Quy định giờ làm việc" });
        await db.SaveChangesAsync();

        var assistant = new KnowledgeAssistant(db, _groqMock.Object, _loggerMock.Object);
        var result = await assistant.AnswerQuestionAsync("Cách deploy kubernetes lên aws", null, null);

        result.Answer.Should().Contain("tôi không tìm thấy tài liệu nội bộ nào phù hợp");
        result.Sources.Should().BeEmpty();
        _groqMock.Verify(g => g.CreateChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AnswerQuestionAsync_MatchingKeywordsAndAcronyms_InvokesGroq()
    {
        using var db = CreateDbContext();
        db.KnowledgeChunks.Add(new KnowledgeChunk { DocumentId = Guid.NewGuid(), DocumentTitle = "IT Policy", Content = "Chính sách thiết bị cho IT" });
        await db.SaveChangesAsync();

        _groqMock.Setup(g => g.CreateChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Nhân viên IT được cấp laptop cấu hình cao.");

        var assistant = new KnowledgeAssistant(db, _groqMock.Object, _loggerMock.Object);
        var result = await assistant.AnswerQuestionAsync("Chính sách cấp thiết bị cho IT?", null, null);

        result.Answer.Should().Be("Nhân viên IT được cấp laptop cấu hình cao.");
        result.Sources.Should().Contain("IT Policy");
    }

    [Fact]
    public async Task AnswerQuestionAsync_RestrictedDepartmentAndRole_IsolatesUnauthorizedChunks()
    {
        var itDeptId = Guid.NewGuid();
        var hrDeptId = Guid.NewGuid();
        var adminRoleId = Guid.NewGuid();

        using var db = CreateDbContext();
        db.KnowledgeChunks.AddRange(
            new KnowledgeChunk { DocumentId = Guid.NewGuid(), DocumentTitle = "IT Secret", Content = "Mật khẩu mạng nội bộ IT", DepartmentId = itDeptId },
            new KnowledgeChunk { DocumentId = Guid.NewGuid(), DocumentTitle = "HR Doc", Content = "Chính sách tuyển dụng", DepartmentId = hrDeptId },
            new KnowledgeChunk { DocumentId = Guid.NewGuid(), DocumentTitle = "Admin Key", Content = "Khóa hệ thống quản trị", RoleId = adminRoleId }
        );
        await db.SaveChangesAsync();

        var assistant = new KnowledgeAssistant(db, _groqMock.Object, _loggerMock.Object);
        // User is only in HR department without admin role
        var result = await assistant.AnswerQuestionAsync("Mật khẩu mạng nội bộ IT?", hrDeptId, Array.Empty<Guid>());

        result.Answer.Should().Contain("tôi không tìm thấy tài liệu nội bộ nào phù hợp");
        result.Sources.Should().BeEmpty();
    }
}
