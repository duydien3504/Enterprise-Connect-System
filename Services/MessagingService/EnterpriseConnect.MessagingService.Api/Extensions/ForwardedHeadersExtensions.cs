using Microsoft.AspNetCore.HttpOverrides;

namespace EnterpriseConnect.MessagingService.Api.Extensions;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddMessagingForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor 
                                     | ForwardedHeaders.XForwardedProto 
                                     | ForwardedHeaders.XForwardedHost;
        });

        return services;
    }
}
