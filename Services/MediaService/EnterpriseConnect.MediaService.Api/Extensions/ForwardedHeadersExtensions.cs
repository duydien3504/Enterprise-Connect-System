using Microsoft.AspNetCore.HttpOverrides;

namespace EnterpriseConnect.MediaService.Api.Extensions;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddMediaForwardedHeaders(this IServiceCollection services)
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
