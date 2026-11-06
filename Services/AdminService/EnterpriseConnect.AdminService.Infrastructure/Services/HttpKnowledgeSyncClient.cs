using System.Net.Http.Json;
using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.AdminService.Infrastructure.Services;

public class HttpKnowledgeSyncClient : IKnowledgeSyncClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpKnowledgeSyncClient> _logger;
    private readonly string _messagingServiceBaseUrl;

    public HttpKnowledgeSyncClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<HttpKnowledgeSyncClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _messagingServiceBaseUrl = configuration["Services:MessagingServiceUrl"] ?? "http://localhost:5166";
    }

    public async Task SyncKnowledgeChunksAsync(SyncKnowledgeChunksDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_messagingServiceBaseUrl.TrimEnd('/')}/api/Knowledge/sync";
            using var response = await _httpClient.PostAsJsonAsync(endpoint, dto, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to sync knowledge chunks to MessagingService. DocumentId: {DocumentId}, StatusCode: {StatusCode}",
                    dto.DocumentId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error syncing knowledge chunks to MessagingService for DocumentId {DocumentId}",
                dto.DocumentId);
        }
    }
}
