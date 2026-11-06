using EnterpriseConnect.AdminService.Application.Common.Services;
using FluentAssertions;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class DocumentChunkerTests
{
    private readonly DocumentChunker _chunker = new();

    [Fact]
    public void ChunkText_NullOrEmptyText_ShouldReturnEmptyList()
    {
        var resultNull = _chunker.ChunkText(null);
        var resultEmpty = _chunker.ChunkText("   ");

        resultNull.Should().BeEmpty();
        resultEmpty.Should().BeEmpty();
    }

    [Fact]
    public void ChunkText_ShortText_ShouldReturnSingleChunk()
    {
        var text = "Chính sách bảo mật nội bộ doanh nghiệp năm 2026.";
        var result = _chunker.ChunkText(text, maxChunkSize: 500);

        result.Should().HaveCount(1);
        result[0].Should().Be(text);
    }

    [Fact]
    public void ChunkText_LongText_ShouldSplitIntoMultipleChunksWithOverlap()
    {
        var sentence = "Đây là điều khoản quan trọng trong tài liệu bảo mật nội bộ của doanh nghiệp. ";
        var longText = string.Concat(Enumerable.Repeat(sentence, 20));

        var result = _chunker.ChunkText(longText, maxChunkSize: 200, overlap: 30);

        result.Count.Should().BeGreaterThan(1);
        foreach (var chunk in result)
        {
            chunk.Length.Should().BeLessThanOrEqualTo(260);
            chunk.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void ChunkText_EdgeCaseNoBoundaries_ShouldStillChunkCorrectly()
    {
        var textWithoutSpaces = new string('A', 350);
        var result = _chunker.ChunkText(textWithoutSpaces, maxChunkSize: 100, overlap: 10);

        result.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void ChunkText_ExactBoundaryOverlap_ShouldNotProduceDuplicateTailChunk()
    {
        // 600 characters with 500 maxChunkSize and 50 overlap should produce exactly 2 chunks, not 3
        var text = new string('X', 600);
        var result = _chunker.ChunkText(text, maxChunkSize: 500, overlap: 50);

        result.Should().HaveCount(2);
        result[0].Length.Should().Be(500);
        result[1].Length.Should().Be(150);
    }
}
