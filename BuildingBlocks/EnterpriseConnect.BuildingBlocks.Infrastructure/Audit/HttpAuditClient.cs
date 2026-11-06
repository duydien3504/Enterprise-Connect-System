using System.Net.Http.Json;
using EnterpriseConnect.BuildingBlocks.Application.Audit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.BuildingBlocks.Infrastructure.Audit;

public class HttpAuditClient : IAuditClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpAuditClient> _logger;
    private readonly string _adminServiceBaseUrl;

    public HttpAuditClient(HttpClient httpClient, IConfiguration configuration, ILogger<HttpAuditClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _adminServiceBaseUrl = configuration["Services:AdminServiceUrl"] ?? "http://localhost:5298";
    }

    public async Task RecordEventAsync(AuditEventDto auditEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_adminServiceBaseUrl.TrimEnd('/')}/api/AuditLogs/record";
            using var response = await _httpClient.PostAsJsonAsync(endpoint, auditEvent, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to send audit log to AdminService. StatusCode: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            // Non-blocking: Lỗi ghi log audit không được phép làm gián đoạn transaction chính
            _logger.LogError(ex, "Error dispatching audit log to AdminService");
        }
    }
}
