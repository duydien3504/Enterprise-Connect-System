using EnterpriseConnect.RemoteService.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace EnterpriseConnect.RemoteService.UnitTests;

public class SessionCodeGeneratorTests
{
    [Fact]
    public void GenerateCode_ShouldReturnStandardFormat()
    {
        var generator = new SessionCodeGenerator();

        var code = generator.GenerateCode();

        code.Should().NotBeNullOrWhiteSpace();
        code.Length.Should().Be(7);
        code[3].Should().Be('-');

        var parts = code.Split('-');
        parts.Should().HaveCount(2);
        int.TryParse(parts[0], out var part1).Should().BeTrue();
        int.TryParse(parts[1], out var part2).Should().BeTrue();
        part1.Should().BeInRange(100, 999);
        part2.Should().BeInRange(100, 999);
    }

    [Fact]
    public void GenerateCode_MultipleCalls_ShouldProduceVariedCodes()
    {
        var generator = new SessionCodeGenerator();
        var codes = Enumerable.Range(0, 50).Select(_ => generator.GenerateCode()).ToHashSet();

        codes.Count.Should().BeGreaterThan(40);
    }
}
