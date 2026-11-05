using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EnterpriseConnect.IdentityService.Infrastructure.Services;

public class PublicUrlProvider : IPublicUrlProvider
{
    private const string DefaultGatewayBaseUrl = "https://localhost:7272";
    private const string DefaultIdentityPrefix = "/identity";

    public PublicUrlProvider(IConfiguration configuration)
    {
        var gatewayBaseUrl = (configuration["PublicUrls:GatewayBaseUrl"] ?? DefaultGatewayBaseUrl).TrimEnd('/');
        var identityPrefix = (configuration["PublicUrls:IdentityPathPrefix"] ?? DefaultIdentityPrefix).Trim('/');

        IdentityBaseUrl = string.IsNullOrEmpty(identityPrefix)
            ? gatewayBaseUrl
            : $"{gatewayBaseUrl}/{identityPrefix}";
    }

    public string IdentityBaseUrl { get; }
}
