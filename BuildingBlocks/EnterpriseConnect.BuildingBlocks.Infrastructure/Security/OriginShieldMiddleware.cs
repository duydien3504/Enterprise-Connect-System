using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.BuildingBlocks.Infrastructure.Security;

public class OriginShieldMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<OriginShieldMiddleware> _logger;
    private readonly string? _expectedSecret;

    public OriginShieldMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<OriginShieldMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _expectedSecret = configuration["Security:GatewaySharedSecret"] 
            ?? Environment.GetEnvironmentVariable("GATEWAY_SHARED_SECRET");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(_expectedSecret))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (path == "/" || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Gateway-Secret", out var headerValue) ||
            string.IsNullOrWhiteSpace(headerValue.ToString()))
        {
            _logger.LogWarning("OriginShield: Rejected request to {Path} from {IP} - Missing X-Gateway-Secret.", 
                path, context.Connection.RemoteIpAddress);
            await ReturnForbiddenAsync(context);
            return;
        }

        var clientBytes = Encoding.UTF8.GetBytes(headerValue.ToString());
        var expectedBytes = Encoding.UTF8.GetBytes(_expectedSecret);

        if (!CryptographicOperations.FixedTimeEquals(clientBytes, expectedBytes))
        {
            _logger.LogWarning("OriginShield: Rejected request to {Path} from {IP} - Invalid X-Gateway-Secret.", 
                path, context.Connection.RemoteIpAddress);
            await ReturnForbiddenAsync(context);
            return;
        }

        await _next(context);
    }

    private static async Task ReturnForbiddenAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Direct access forbidden. Requests must originate from API Gateway.\",\"statusCode\":403}");
    }
}
