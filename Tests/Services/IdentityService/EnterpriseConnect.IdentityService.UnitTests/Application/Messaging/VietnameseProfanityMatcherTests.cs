using EnterpriseConnect.MessagingService.Infrastructure.Services.Ai;
using FluentAssertions;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Application.Messaging;

public class VietnameseProfanityMatcherTests
{
    [Theory]
    [InlineData("thằng chó này", true, "Medium")]
    [InlineData("đồ chó đẻ", true, "Medium")]
    [InlineData("óc chó thật sự", true, "Medium")]
    [InlineData("thằng ngu dốt", true, "Medium")]
    [InlineData("khốn nạn vcl", true, "High")]
    [InlineData("địt mẹ mày", true, "High")]
    [InlineData("đụ má", true, "High")]
    [InlineData("đéo quan tâm", true, "Medium")]
    [InlineData("mất dạy", true, "Medium")]
    [InlineData("con đĩ", true, "High")]
    [InlineData("dm", true, "High")]
    [InlineData("vkl", true, "High")]
    [InlineData("clgt", true, "High")]
    public void Match_WhenInsultingContent_ShouldDetectToxicWithSeverity(string input, bool expectedToxic, string expectedSeverity)
    {
        var result = VietnameseProfanityMatcher.Match(input);

        result.IsMatched.Should().Be(expectedToxic);
        result.Severity.Should().Be(expectedSeverity);
        result.Reason.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("Xin chào mọi người")]
    [InlineData("Hôm nay chúng ta họp lúc 9h sáng")]
    [InlineData("Dự án đang tiến triển rất tốt")]
    [InlineData("Cảm ơn anh đã hỗ trợ")]
    public void Match_WhenPoliteContent_ShouldReturnNotMatched(string input)
    {
        var result = VietnameseProfanityMatcher.Match(input);

        result.IsMatched.Should().BeFalse();
        result.Severity.Should().Be("None");
        result.Reason.Should().BeEmpty();
    }
}
