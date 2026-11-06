namespace EnterpriseConnect.AdminService.Application.Common.Interfaces;

public interface IDocumentChunker
{
    IReadOnlyList<string> ChunkText(string? text, int maxChunkSize = 500, int overlap = 50);
}
