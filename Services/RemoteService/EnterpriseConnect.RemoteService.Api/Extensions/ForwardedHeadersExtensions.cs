using Microsoft.AspNetCore.HttpOverrides;

namespace EnterpriseConnect.RemoteService.Api.Extensions;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddRemoteForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }
}
