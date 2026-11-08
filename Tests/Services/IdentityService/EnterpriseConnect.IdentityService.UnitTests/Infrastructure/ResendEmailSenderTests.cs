using System.Net;
using EnterpriseConnect.IdentityService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Resend;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Infrastructure;

public class ResendEmailSenderTests
{
    private readonly Mock<ILogger<ResendEmailSender>> _loggerMock;
    private readonly Mock<IResend> _resendMock;

    public ResendEmailSenderTests()
    {
        _loggerMock = new Mock<ILogger<ResendEmailSender>>();
        _resendMock = new Mock<IResend>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("YOUR_RESEND_API_KEY_HERE")]
    [InlineData("localhost")]
    public async Task SendEmailAsync_WhenApiKeyIsMissingOrPlaceholder_ShouldNotCallResendClient(string? apiKey)
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ResendSettings:ApiKey", apiKey },
            { "ResendSettings:FromEmail", "onboarding@resend.dev" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var sender = new ResendEmailSender(configuration, _loggerMock.Object, _resendMock.Object);

        // Act
        var act = () => sender.SendEmailAsync("test@example.com", "Test Subject", "<p>Test</p>");

        // Assert
        await act.Should().NotThrowAsync();
        _resendMock.Verify(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_WhenApiKeyIsValid_ShouldInvokeResendWithCorrectPayload()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ResendSettings:ApiKey", "re_test_valid_key_123" },
            { "ResendSettings:FromEmail", "onboarding@resend.dev" },
            { "ResendSettings:FromName", "Enterprise Connect" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        EmailMessage? capturedMessage = null;

        _resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((msg, ct) => capturedMessage = msg)
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null!));

        var sender = new ResendEmailSender(configuration, _loggerMock.Object, _resendMock.Object);

        // Act
        await sender.SendEmailAsync("recipient@example.com", "Mã xác thực", "<p>123456</p>");

        // Assert
        _resendMock.Verify(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        capturedMessage.Should().NotBeNull();
        capturedMessage!.From.ToString().Should().Be("Enterprise Connect <onboarding@resend.dev>");
        capturedMessage.To.Select(x => x.Email).Should().Contain("recipient@example.com");
        capturedMessage.Subject.Should().Be("Mã xác thực");
        capturedMessage.HtmlBody.Should().Be("<p>123456</p>");
    }

    [Fact]
    public async Task SendEmailAsync_WhenFromNameIsEmpty_ShouldUseFromEmailDirectly()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ResendSettings:ApiKey", "re_test_valid_key_123" },
            { "ResendSettings:FromEmail", "custom@hugonef.id.vn" },
            { "ResendSettings:FromName", "" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        EmailMessage? capturedMessage = null;

        _resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((msg, ct) => capturedMessage = msg)
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null!));

        var sender = new ResendEmailSender(configuration, _loggerMock.Object, _resendMock.Object);

        // Act
        await sender.SendEmailAsync("user@example.com", "Hello", "Content");

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage!.From.ToString().Should().Be("custom@hugonef.id.vn");
    }

    [Fact]
    public async Task SendEmailAsync_WhenResendResponseReportsFailure_ShouldLogAndNotThrow()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ResendSettings:ApiKey", "re_test_valid_key_123" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var resendException = new ResendException(HttpStatusCode.Forbidden, ErrorType.ValidationError, "Domain is not verified", null!);

        _resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(resendException, null!));

        var sender = new ResendEmailSender(configuration, _loggerMock.Object, _resendMock.Object);

        // Act
        var act = () => sender.SendEmailAsync("test@example.com", "Test Subject", "<p>Test</p>");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendEmailAsync_WhenResendThrowsException_ShouldCatchAndNotThrow()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ResendSettings:ApiKey", "re_test_valid_key_123" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network failure"));

        var sender = new ResendEmailSender(configuration, _loggerMock.Object, _resendMock.Object);

        // Act
        var act = () => sender.SendEmailAsync("test@example.com", "Test Subject", "<p>Test</p>");

        // Assert
        await act.Should().NotThrowAsync();
    }
}
