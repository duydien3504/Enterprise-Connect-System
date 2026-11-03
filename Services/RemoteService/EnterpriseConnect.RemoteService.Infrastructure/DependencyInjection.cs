using EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;
using EnterpriseConnect.RemoteService.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseConnect.RemoteService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRemoteInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPostgresDbContext<RemoteDbContext>(
            configuration,
            connectionStringName: "RemoteDb",
            migrationsAssembly: typeof(RemoteDbContext).Assembly.FullName);

        return services;
    }
}
