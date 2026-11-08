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
        var smtpHost = _configuration["SmtpSettings:Host"] ?? "localhost";
        var smtpPort = int.TryParse(_configuration["SmtpSettings:Port"], out var port) ? port : 587;
        var smtpUser = _configuration["SmtpSettings:Username"];
        var smtpPass = _configuration["SmtpSettings:Password"];
        var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? "no-reply@enterpriseconnect.com";
        var senderName = _configuration["SmtpSettings:SenderName"] ?? "Enterprise Connect";

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

            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);
            if (!string.IsNullOrEmpty(smtpUser))
            {
                await client.AuthenticateAsync(smtpUser, smtpPass, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email qua SMTP tới {ToEmail}", toEmail);
            // Trong production có thể throw hoặc retry, ở đây log để đảm bảo API không bị crash
        }
    }
}
