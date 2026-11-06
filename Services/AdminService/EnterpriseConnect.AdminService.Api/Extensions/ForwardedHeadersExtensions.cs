using Microsoft.AspNetCore.HttpOverrides;

namespace EnterpriseConnect.AdminService.Api.Extensions;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddAdminForwardedHeaders(this IServiceCollection services)
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
