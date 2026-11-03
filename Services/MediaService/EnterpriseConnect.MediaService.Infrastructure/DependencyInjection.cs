using EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;
using EnterpriseConnect.MediaService.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseConnect.MediaService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMediaInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPostgresDbContext<MediaDbContext>(
            configuration,
            connectionStringName: "MediaDb",
            migrationsAssembly: typeof(MediaDbContext).Assembly.FullName);

        return services;
    }
}
