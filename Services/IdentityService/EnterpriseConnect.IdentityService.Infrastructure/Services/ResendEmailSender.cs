using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Resend;

namespace EnterpriseConnect.IdentityService.Infrastructure.Services;

public class ResendEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendEmailSender> _logger;
    private readonly IResend? _resendClient;

    [ActivatorUtilitiesConstructor]
    public ResendEmailSender(
        IConfiguration configuration,
        ILogger<ResendEmailSender> logger) : this(configuration, logger, null)
    {
    }

    public ResendEmailSender(
        IConfiguration configuration,
        ILogger<ResendEmailSender> logger,
        IResend? resendClient)
    {
        _configuration = configuration;
        _logger = logger;
        _resendClient = resendClient;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["ResendSettings:ApiKey"]?.Trim('"', '\'', ' ');
        var fromEmail = (_configuration["ResendSettings:FromEmail"] ?? "onboarding@resend.dev").Trim('"', '\'', ' ');
        var fromName = (_configuration["ResendSettings:FromName"] ?? "Enterprise Connect").Trim('"', '\'', ' ');

        // Kiểm tra nếu API key chưa được cấu hình hoặc là chuỗi mẫu mặc định
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("YOUR_RESEND_API_KEY") || apiKey == "localhost")
        {
            _logger.LogWarning("[Resend Simulation] ApiKey chưa được cấu hình hợp lệ. Mô phỏng gửi tới {ToEmail} | Subject: {Subject} | Body preview: {Body}",
                toEmail, subject, bodyHtml.Length > 120 ? bodyHtml.Substring(0, 120) + "..." : bodyHtml);
            return;
        }

        try
        {
            var resend = _resendClient ?? ResendClient.Create(apiKey);

            var senderDisplay = string.IsNullOrWhiteSpace(fromName) ? fromEmail : $"{fromName} <{fromEmail}>";

            var emailMessage = new EmailMessage
            {
                From = senderDisplay,
                To = toEmail,
                Subject = subject,
                HtmlBody = bodyHtml
            };

            var response = await resend.EmailSendAsync(emailMessage, cancellationToken);
            if (response.Success)
            {
                _logger.LogInformation("✓ Đã gửi email thành công qua Resend HTTP API tới {ToEmail} (EmailId: {EmailId})", toEmail, response.Content);
            }
            else
            {
                _logger.LogError("Lỗi khi gửi email qua Resend HTTP API tới {ToEmail}: {Error}. " +
                    "Lưu ý: Nếu đang dùng 'onboarding@resend.dev', Resend chỉ cho phép gửi đến email đã đăng ký tài khoản Resend.",
                    toEmail, response.Exception?.Message ?? "Không xác định");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi Resend HTTP API tới {ToEmail}", toEmail);
        }
    }
}
