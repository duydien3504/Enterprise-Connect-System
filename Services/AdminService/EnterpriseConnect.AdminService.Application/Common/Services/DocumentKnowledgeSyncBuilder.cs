using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Entities;

namespace EnterpriseConnect.AdminService.Application.Common.Services;

public static class DocumentKnowledgeSyncBuilder
{
    public static SyncKnowledgeChunksDto BuildSyncDto(Document doc, string? rawText, IDocumentChunker? chunker)
    {
        var textToChunk = !string.IsNullOrWhiteSpace(rawText)
            ? rawText
            : (!string.IsNullOrWhiteSpace(doc.Description) ? doc.Description : doc.Title);

        var chunks = chunker != null
            ? chunker.ChunkText(textToChunk)
            : (!string.IsNullOrWhiteSpace(textToChunk) ? new[] { textToChunk } : Array.Empty<string>());

        var chunkItems = new List<KnowledgeChunkItemDto>();

        foreach (var chunk in chunks)
        {
            if (doc.AccessList != null && doc.AccessList.Count > 0)
            {
                foreach (var access in doc.AccessList)
                {
                    chunkItems.Add(new KnowledgeChunkItemDto
                    {
                        Content = chunk,
                        DepartmentId = access.DepartmentId,
                        RoleId = access.RoleId
                    });
                }
            }
            else
            {
                chunkItems.Add(new KnowledgeChunkItemDto
                {
                    Content = chunk,
                    DepartmentId = null,
                    RoleId = null
                });
            }
        }

        return new SyncKnowledgeChunksDto
        {
            DocumentId = doc.Id,
            DocumentTitle = doc.Title,
            Chunks = chunkItems
        };
    }
}
