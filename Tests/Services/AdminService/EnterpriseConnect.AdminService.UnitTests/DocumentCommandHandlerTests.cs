using EnterpriseConnect.AdminService.Application.Common.Interfaces;
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

public class DocumentCommandHandlerTests
{
    private readonly Mock<IAdminUnitOfWork> _uowMock = new();
    private readonly Mock<IDocumentRepository> _repoMock = new();
    private readonly Mock<ICloudinaryStorageService> _cloudinaryMock = new();

    public DocumentCommandHandlerTests()
    {
        _uowMock.Setup(u => u.Documents).Returns(_repoMock.Object);
    }

    [Fact]
    public async Task Upload_ValidCommand_ShouldSaveAndReturnDto()
    {
        var handler = new UploadDocumentCommandHandler(_uowMock.Object, _cloudinaryMock.Object);
        var command = new UploadDocumentCommand
        {
            UploaderId = Guid.NewGuid(),
            Title = "Security Policy",
            FileUrl = "https://minio/docs/policy.pdf",
            FileType = DocumentType.PDF,
            Version = "1.0",
            FileSizeBytes = 1024,
            RoleIds = new List<Guid> { Guid.NewGuid() }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.Title.Should().Be("Security Policy");
        result.Data.AccessList.Should().HaveCount(1);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_PartialFields_ShouldOnlyUpdateProvidedFields()
    {
        var doc = new Document { Id = Guid.NewGuid(), Title = "Old Title", Version = "1.0", Description = "Old Desc" };
        _repoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var handler = new UpdateDocumentCommandHandler(_uowMock.Object);
        var command = new UpdateDocumentCommand { Id = doc.Id, Title = "New Title" }; // Only Title, Version is omitted!

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.Title.Should().Be("New Title");
        result.Data.Version.Should().Be("1.0"); // Preserved!
        result.Data.Description.Should().Be("Old Desc"); // Preserved!
    }

    [Fact]
    public async Task UpdateStatus_ValidDoc_ShouldUpdateToIndexed()
    {
        var doc = new Document { Id = Guid.NewGuid(), Title = "Doc1", Status = DocumentStatus.Uploaded };
        _repoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var handler = new UpdateDocumentStatusCommandHandler(_uowMock.Object);
        var result = await handler.Handle(new UpdateDocumentStatusCommand { Id = doc.Id, Status = DocumentStatus.Indexed }, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be(DocumentStatus.Indexed);
    }

    [Fact]
    public async Task Delete_ValidDoc_ShouldDelete()
    {
        var doc = new Document { Id = Guid.NewGuid(), Title = "DocToDelete" };
        _repoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var handler = new DeleteDocumentCommandHandler(_uowMock.Object);
        var result = await handler.Handle(new DeleteDocumentCommand(doc.Id), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteAsync(doc, It.IsAny<CancellationToken>()), Times.Once);
    }
}
