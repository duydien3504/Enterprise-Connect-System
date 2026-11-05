using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.RemoteService.Infrastructure.Services;

public class SessionExpirationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionExpirationBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public SessionExpirationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SessionExpirationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SessionExpirationBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IRemoteUnitOfWork>();

                var expiredSessions = await unitOfWork.RemoteSessions.GetExpiredSessionsAsync(
                    DateTimeOffset.UtcNow, stoppingToken);

                if (expiredSessions.Count != 0)
                {
                    _logger.LogInformation("Found {Count} expired remote sessions. Updating status...", expiredSessions.Count);

                    foreach (var session in expiredSessions)
                    {
                        session.Status = RemoteSessionStatus.Expired;
                        session.EndedAt = DateTimeOffset.UtcNow;
                        session.EndReason = "Session expired due to inactivity or TTL timeout.";
                        await unitOfWork.RemoteSessions.UpdateAsync(session, stoppingToken);
                    }

                    await unitOfWork.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while expiring remote sessions.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
