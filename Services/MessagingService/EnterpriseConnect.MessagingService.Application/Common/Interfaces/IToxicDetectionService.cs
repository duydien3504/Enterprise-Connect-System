namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public interface IToxicDetectionService
{
    Task<ToxicCheckResult> AnalyzeContentAsync(string content, CancellationToken cancellationToken = default);
}
