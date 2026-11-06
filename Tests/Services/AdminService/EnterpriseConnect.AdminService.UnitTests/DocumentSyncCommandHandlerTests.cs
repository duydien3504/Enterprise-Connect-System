using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Application.Common.Services;
using EnterpriseConnect.AdminService.Application.Features.Documents.Commands.DeleteDocument;
using EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UpdateDocument;
using EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UpdateDocumentStatus;
using EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UploadDocument;
using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class DocumentSyncCommandHandlerTests
{
    private readonly Mock<IAdminUnitOfWork> _uowMock = new();
    private readonly Mock<IDocumentRepository> _repoMock = new();
    private readonly Mock<ICloudinaryStorageService> _cloudinaryMock = new();
    private readonly Mock<IKnowledgeSyncClient> _syncClientMock = new();
    private readonly IDocumentChunker _chunker = new DocumentChunker();

    public DocumentSyncCommandHandlerTests()
    {
        _uowMock.Setup(u => u.Documents).Returns(_repoMock.Object);
    }

    [Fact]
    public async Task Upload_WithSyncClient_ShouldInvokeSyncKnowledgeChunksAsync()
    {
        var handler = new UploadDocumentCommandHandler(_uowMock.Object, _cloudinaryMock.Object, _syncClientMock.Object, _chunker);
        var command = new UploadDocumentCommand
        {
            UploaderId = Guid.NewGuid(),
            Title = "Company Guidelines",
            Description = "All employees must follow guidelines.",
            FileUrl = "https://minio/docs/guidelines.pdf"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        _syncClientMock.Verify(s => s.SyncKnowledgeChunksAsync(
            It.Is<SyncKnowledgeChunksDto>(dto => dto.DocumentTitle == "Company Guidelines" && dto.Chunks.Count > 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithSyncClient_ShouldInvokeSyncKnowledgeChunksAsync()
    {
        var doc = new Document { Id = Guid.NewGuid(), Title = "Old Title", Description = "Old Content" };
        _repoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var handler = new UpdateDocumentCommandHandler(_uowMock.Object, _syncClientMock.Object, _chunker);
        var command = new UpdateDocumentCommand { Id = doc.Id, Title = "New Title", Description = "Updated Content" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        _syncClientMock.Verify(s => s.SyncKnowledgeChunksAsync(
            It.Is<SyncKnowledgeChunksDto>(dto => dto.DocumentId == doc.Id && dto.DocumentTitle == "New Title"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_ToIndexed_ShouldInvokeSyncKnowledgeChunksAsync()
    {
        var doc = new Document { Id = Guid.NewGuid(), Title = "Doc Ready", Description = "Content ready to index", Status = DocumentStatus.Uploaded };
        _repoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var handler = new UpdateDocumentStatusCommandHandler(_uowMock.Object, _syncClientMock.Object, _chunker);
        var result = await handler.Handle(new UpdateDocumentStatusCommand { Id = doc.Id, Status = DocumentStatus.Indexed }, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        _syncClientMock.Verify(s => s.SyncKnowledgeChunksAsync(
            It.Is<SyncKnowledgeChunksDto>(dto => dto.DocumentId == doc.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_ToNonIndexed_ShouldPurgeChunksBySyncingEmptyList()
    {
        var doc = new Document { Id = Guid.NewGuid(), Title = "Doc Archive", Status = DocumentStatus.Indexed };
        _repoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var handler = new UpdateDocumentStatusCommandHandler(_uowMock.Object, _syncClientMock.Object, _chunker);
        var result = await handler.Handle(new UpdateDocumentStatusCommand { Id = doc.Id, Status = DocumentStatus.Failed }, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        _syncClientMock.Verify(s => s.SyncKnowledgeChunksAsync(
            It.Is<SyncKnowledgeChunksDto>(dto => dto.DocumentId == doc.Id && dto.Chunks.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithSyncClient_ShouldPurgeChunksBySyncingEmptyList()
    {
        var doc = new Document { Id = Guid.NewGuid(), Title = "DocToDelete" };
        _repoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var handler = new DeleteDocumentCommandHandler(_uowMock.Object, _syncClientMock.Object);
        var result = await handler.Handle(new DeleteDocumentCommand(doc.Id), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        _syncClientMock.Verify(s => s.SyncKnowledgeChunksAsync(
            It.Is<SyncKnowledgeChunksDto>(dto => dto.DocumentId == doc.Id && dto.Chunks.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
