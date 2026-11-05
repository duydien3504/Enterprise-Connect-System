namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default);
}
