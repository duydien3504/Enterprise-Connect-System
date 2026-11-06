using EnterpriseConnect.BuildingBlocks.Application.Audit;
using EnterpriseConnect.BuildingBlocks.Infrastructure.Audit;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;

public static class AuditExtensions
{
    public static IServiceCollection AddDistributedAuditClient(this IServiceCollection services)
    {
        services.AddHttpClient<IAuditClient, HttpAuditClient>();
        return services;
    }
}
