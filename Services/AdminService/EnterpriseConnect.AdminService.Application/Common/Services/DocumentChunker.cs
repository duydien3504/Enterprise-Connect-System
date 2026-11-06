using EnterpriseConnect.AdminService.Application.Common.Interfaces;

namespace EnterpriseConnect.AdminService.Application.Common.Services;

public class DocumentChunker : IDocumentChunker
{
    public IReadOnlyList<string> ChunkText(string? text, int maxChunkSize = 500, int overlap = 50)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var normalized = text.Trim();
        maxChunkSize = Math.Max(100, maxChunkSize);
        overlap = Math.Clamp(overlap, 0, maxChunkSize / 2);

        if (normalized.Length <= maxChunkSize)
        {
            return new[] { normalized };
        }

        var chunks = new List<string>();
        int start = 0;

        while (start < normalized.Length)
        {
            int length = Math.Min(maxChunkSize, normalized.Length - start);

            if (start + length < normalized.Length)
            {
                int boundarySearchWindow = Math.Min(60, length);
                int lastBoundary = normalized.LastIndexOfAny(new[] { ' ', '\n', '\r', '.', ';' }, start + length - 1, boundarySearchWindow);
                if (lastBoundary > start)
                {
                    length = (lastBoundary - start) + 1;
                }
            }

            var chunk = normalized.Substring(start, length).Trim();
            if (!string.IsNullOrEmpty(chunk))
            {
                chunks.Add(chunk);
            }

            if (start + length >= normalized.Length)
            {
                break;
            }

            int step = Math.Max(1, length - overlap);
            start += step;
        }

        return chunks;
    }
}
