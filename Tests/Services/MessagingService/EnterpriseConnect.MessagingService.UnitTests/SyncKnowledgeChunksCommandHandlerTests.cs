using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Knowledge.Commands.SyncKnowledgeChunks;
using EnterpriseConnect.MessagingService.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.MessagingService.UnitTests;

public class SyncKnowledgeChunksCommandHandlerTests
{
    private readonly Mock<IMessagingUnitOfWork> _uowMock = new();
    private readonly Mock<IKnowledgeChunkRepository> _chunkRepoMock = new();

    public SyncKnowledgeChunksCommandHandlerTests()
    {
        _uowMock.Setup(u => u.KnowledgeChunks).Returns(_chunkRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ValidChunks_ShouldReplaceAndSaveEntities()
    {
        var docId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var handler = new SyncKnowledgeChunksCommandHandler(_uowMock.Object);
        var command = new SyncKnowledgeChunksCommand
        {
            DocumentId = docId,
            DocumentTitle = "HR Guidelines",
            Chunks = new List<SyncKnowledgeChunkItem>
            {
                new() { Content = "Part 1: Working hours", DepartmentId = deptId, RoleId = roleId },
                new() { Content = "Part 2: Remote policy", DepartmentId = null, RoleId = null },
                new() { Content = "   " } // Empty chunk should be skipped!
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.DocumentId.Should().Be(docId);
        result.ChunksCount.Should().Be(2);

        _chunkRepoMock.Verify(r => r.ReplaceChunksAsync(
            docId,
            It.Is<IEnumerable<KnowledgeChunk>>(chunks => chunks.Count() == 2 && chunks.All(c => c.DocumentId == docId && c.DocumentTitle == "HR Guidelines")),
            It.IsAny<CancellationToken>()), Times.Once);

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyChunksList_ShouldCallReplaceWithEmptyAndSave()
    {
        var docId = Guid.NewGuid();
        var handler = new SyncKnowledgeChunksCommandHandler(_uowMock.Object);
        var command = new SyncKnowledgeChunksCommand
        {
            DocumentId = docId,
            DocumentTitle = "Empty Doc",
            Chunks = new List<SyncKnowledgeChunkItem>()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ChunksCount.Should().Be(0);

        _chunkRepoMock.Verify(r => r.ReplaceChunksAsync(
            docId,
            It.Is<IEnumerable<KnowledgeChunk>>(chunks => !chunks.Any()),
            It.IsAny<CancellationToken>()), Times.Once);

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
