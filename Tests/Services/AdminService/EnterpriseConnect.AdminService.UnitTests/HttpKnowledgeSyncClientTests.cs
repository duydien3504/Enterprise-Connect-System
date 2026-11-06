using System.Net;
using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class HttpKnowledgeSyncClientTests
{
    private readonly Mock<ILogger<HttpKnowledgeSyncClient>> _loggerMock = new();
    private readonly Mock<IConfiguration> _configMock = new();

    public HttpKnowledgeSyncClientTests()
    {
        _configMock.Setup(c => c["Services:MessagingServiceUrl"]).Returns("http://localhost:5166");
    }

    [Fact]
    public async Task SyncKnowledgeChunksAsync_SuccessfulResponse_ShouldSendPostRequest()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new HttpKnowledgeSyncClient(httpClient, _configMock.Object, _loggerMock.Object);

        var dto = new SyncKnowledgeChunksDto
        {
            DocumentId = Guid.NewGuid(),
            DocumentTitle = "HR Policy",
            Chunks = new List<KnowledgeChunkItemDto> { new() { Content = "Rule 1" } }
        };

        var act = async () => await client.SyncKnowledgeChunksAsync(dto, CancellationToken.None);
        await act.Should().NotThrowAsync();

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.ToString().Contains("/api/Knowledge/sync")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SyncKnowledgeChunksAsync_WhenHttpThrowsException_ShouldBeResilientAndNotThrow()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(handlerMock.Object);
        var client = new HttpKnowledgeSyncClient(httpClient, _configMock.Object, _loggerMock.Object);

        var dto = new SyncKnowledgeChunksDto { DocumentId = Guid.NewGuid() };

        var act = async () => await client.SyncKnowledgeChunksAsync(dto, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
