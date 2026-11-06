using EnterpriseConnect.AdminService.Application.Features.AuditLogs.Commands.RecordAuditLog;
using EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UpdateDocument;
using EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UploadDocument;
using FluentAssertions;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class AdminValidatorsTests
{
    [Fact]
    public void RecordAuditLogValidator_EmptyCorrelationId_ShouldFail()
    {
        var validator = new RecordAuditLogValidator();
        var command = new RecordAuditLogCommand
        {
            CorrelationId = "",
            ServiceName = "MessagingService",
            ActionName = "SendMessage",
            TargetEntity = "Message",
            ActorId = Guid.NewGuid(),
            IpAddress = "127.0.0.1",
            HttpMethod = "POST",
            EndpointUrl = "/api/messages",
            StatusCode = 200
        };

        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RecordAuditLogCommand.CorrelationId));
    }

    [Fact]
    public void UploadDocumentValidator_NoFileAndNoFileUrl_ShouldFail()
    {
        var validator = new UploadDocumentValidator();
        var command = new UploadDocumentCommand
        {
            UploaderId = Guid.NewGuid(),
            Version = "1.0"
        };

        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UploadDocumentValidator_InvalidExtension_ShouldFail()
    {
        var validator = new UploadDocumentValidator();
        var command = new UploadDocumentCommand
        {
            UploaderId = Guid.NewGuid(),
            FileName = "malicious.exe",
            FileStream = new MemoryStream(new byte[] { 1, 2, 3 }),
            Version = "1.0"
        };

        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UploadDocumentCommand.FileName));
    }

    [Fact]
    public void UpdateDocumentValidator_PartialUpdate_ShouldPass()
    {
        var validator = new UpdateDocumentValidator();
        var command = new UpdateDocumentCommand
        {
            Id = Guid.NewGuid(),
            Title = "Updated Title" // Version and Description omitted
        };

        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
