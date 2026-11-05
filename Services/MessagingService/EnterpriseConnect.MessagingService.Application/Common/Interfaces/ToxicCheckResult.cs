namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public record ToxicCheckResult(bool IsToxic, string Severity, string Reason);
