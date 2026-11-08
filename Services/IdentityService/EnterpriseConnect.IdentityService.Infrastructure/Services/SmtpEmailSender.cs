using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EnterpriseConnect.IdentityService.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default)
    {
        var smtpHost = (_configuration["SmtpSettings:Host"] ?? "localhost").Trim('"', '\'', ' ');
        var rawPort = _configuration["SmtpSettings:Port"]?.Trim('"', '\'', ' ');
        var smtpPort = int.TryParse(rawPort, out var port) ? port : 587;
        var smtpUser = _configuration["SmtpSettings:Username"]?.Trim('"', '\'', ' ');
        var rawPass = _configuration["SmtpSettings:Password"]?.Trim('"', '\'', ' ');
        // Mật khẩu ứng dụng Google (App Password) có thể chứa khoảng trắng dạng 4 cụm, tự động chuẩn hóa
        var smtpPass = rawPass?.Replace(" ", "");
        var senderEmail = (_configuration["SmtpSettings:SenderEmail"] ?? "no-reply@enterpriseconnect.com").Trim('"', '\'', ' ');
        var senderName = (_configuration["SmtpSettings:SenderName"] ?? "Enterprise Connect").Trim('"', '\'', ' ');

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = bodyHtml
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 4000; // Giới hạn 4s timeout để tránh giữ kết nối vô thời hạn
            
            // Nếu không có host cấu hình thật hoặc đang test, log nội dung thay vì throw lỗi làm đứt flow
            if (string.IsNullOrEmpty(smtpUser) || smtpHost == "localhost")
            {
                _logger.LogInformation("[Email Simulation] Gửi tới {ToEmail} | Subject: {Subject} | Body preview: {Body}", 
                    toEmail, subject, bodyHtml.Length > 100 ? bodyHtml.Substring(0, 100) + "..." : bodyHtml);
                return;
            }

            var secureOption = smtpPort == 465 
                ? SecureSocketOptions.SslOnConnect 
                : SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(smtpHost, smtpPort, secureOption, cancellationToken);
            if (!string.IsNullOrEmpty(smtpUser))
            {
                await client.AuthenticateAsync(smtpUser, smtpPass, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            _logger.LogInformation("✓ Đã gửi email OTP thành công qua SMTP Google tới {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email qua SMTP tới {ToEmail}", toEmail);
            // Trong production có thể throw hoặc retry, ở đây log để đảm bảo API không bị crash
        }
    }
}
