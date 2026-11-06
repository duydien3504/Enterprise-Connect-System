using System.Net;

namespace EnterpriseConnect.ApiGateway.Extensions;

public static class PipelineExtensions
{
    public static IApplicationBuilder UseGatewayCorrelationId(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            const string correlationHeader = "X-Correlation-Id";
            if (!context.Request.Headers.TryGetValue(correlationHeader, out var correlationId) || string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
                context.Request.Headers[correlationHeader] = correlationId;
            }
            context.Response.Headers[correlationHeader] = correlationId;
            await next();
        });
    }

    public static IApplicationBuilder UseInternalEndpointShield(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Zero-Trust: Chặn toàn bộ endpoint nội bộ giữa các microservices khi gọi qua Gateway
            var isInternalEndpoint =
                path.StartsWith("/identity/api/Contacts/check-block", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/api/Contacts/check-block", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/admin/api/AuditLogs/record", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/api/AuditLogs/record", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/messaging/api/Knowledge/sync", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/api/Knowledge/sync", StringComparison.OrdinalIgnoreCase);

            if (isInternalEndpoint)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Access to internal microservice endpoint is forbidden via public gateway.\",\"statusCode\":403}");
                return;
            }

            await next();
        });
    }
}
