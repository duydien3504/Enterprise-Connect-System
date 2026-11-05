namespace EnterpriseConnect.IdentityService.Api.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "IdentityCorsPolicy";

    public static IServiceCollection AddIdentityCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                      {
                          if (string.IsNullOrEmpty(origin)) return false;
                          if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                          {
                              return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                                     uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
                          }
                          return false;
                      })
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
