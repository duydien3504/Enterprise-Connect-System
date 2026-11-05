namespace EnterpriseConnect.ApiGateway.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "GatewayCorsPolicy";

    public static IServiceCollection AddGatewayCors(this IServiceCollection services, IConfiguration configuration)
    {
        var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                      {
                          if (string.IsNullOrEmpty(origin)) return false;
                          if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                          {
                              // Tự động cho phép mọi client/Swagger chạy trên localhost hoặc 127.0.0.1 với bất kỳ cổng nào
                              if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                                  uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                              {
                                  return true;
                              }
                          }
                          return configuredOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
                      })
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials() // Bắt buộc cho SignalR WebSockets
                      .WithExposedHeaders("Content-Disposition", "X-Correlation-Id", "Retry-After")
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });

        return services;
    }
}
