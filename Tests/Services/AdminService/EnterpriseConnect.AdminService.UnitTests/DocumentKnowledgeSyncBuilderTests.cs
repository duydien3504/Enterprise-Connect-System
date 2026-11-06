using EnterpriseConnect.AdminService.Application.Common.Services;
using EnterpriseConnect.AdminService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class DocumentKnowledgeSyncBuilderTests
{
    private readonly DocumentChunker _chunker = new();

    [Fact]
    public void BuildSyncDto_PublicDocument_ShouldProduceChunksWithNullDepartmentAndRole()
    {
        var doc = new Document
        {
            Id = Guid.NewGuid(),
            Title = "Public Handbook",
            Description = "General employee guidelines for 2026."
        };

        var result = DocumentKnowledgeSyncBuilder.BuildSyncDto(doc, null, _chunker);

        result.DocumentId.Should().Be(doc.Id);
        result.DocumentTitle.Should().Be("Public Handbook");
        result.Chunks.Should().NotBeEmpty();
        result.Chunks.Should().OnlyContain(c => c.DepartmentId == null && c.RoleId == null);
    }

    [Fact]
    public void BuildSyncDto_RestrictedDocument_ShouldProduceChunksPerAccessGrant()
    {
        var deptId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var doc = new Document
        {
            Id = Guid.NewGuid(),
            Title = "Confidential Financial Report",
            Description = "Q3 financial balance sheet."
        };
        doc.AccessList.Add(new DocumentAccess { DocumentId = doc.Id, DepartmentId = deptId });
        doc.AccessList.Add(new DocumentAccess { DocumentId = doc.Id, RoleId = roleId });

        var result = DocumentKnowledgeSyncBuilder.BuildSyncDto(doc, null, _chunker);

        result.Chunks.Should().HaveCount(2);
        result.Chunks.Should().Contain(c => c.DepartmentId == deptId && c.RoleId == null);
        result.Chunks.Should().Contain(c => c.DepartmentId == null && c.RoleId == roleId);
    }

    [Fact]
    public void BuildSyncDto_WithRawText_ShouldPreferRawTextOverDescription()
    {
        var doc = new Document
        {
            Id = Guid.NewGuid(),
            Title = "Uploaded Manual",
            Description = "Short description fallback."
        };
        var rawContent = "Direct raw text extracted from uploaded markdown stream.";

        var result = DocumentKnowledgeSyncBuilder.BuildSyncDto(doc, rawContent, _chunker);

        result.Chunks.Should().ContainSingle(c => c.Content == rawContent);
    }
}
