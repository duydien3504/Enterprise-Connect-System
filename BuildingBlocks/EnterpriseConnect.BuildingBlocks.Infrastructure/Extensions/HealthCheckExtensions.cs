using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddEnterpriseHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }

    public static IEndpointRouteBuilder MapEnterpriseHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        var options = new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTimeOffset.UtcNow,
                    durationMs = report.TotalDuration.TotalMilliseconds,
                    entries = report.Entries.Select(e => new
                    {
                        key = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        durationMs = e.Value.Duration.TotalMilliseconds
                    })
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        };

        endpoints.MapHealthChecks("/health/live", options);
        endpoints.MapHealthChecks("/health/ready", options);
        endpoints.MapHealthChecks("/health", options);

        return endpoints;
    }
}
