using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace EnterpriseConnect.IdentityService.Api.Extensions;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddIdentityForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            foreach (var proxy in configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? Array.Empty<string>())
            {
                if (IPAddress.TryParse(proxy, out var ip))
                {
                    options.KnownProxies.Add(ip);
                }
            }
        });

        return services;
    }
}
