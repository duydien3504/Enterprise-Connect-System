using EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;
using EnterpriseConnect.MessagingService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseConnect.MessagingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMessagingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPostgresDbContext<MessagingDbContext>(
            configuration,
            connectionStringName: "MessagingDb",
            migrationsAssembly: typeof(MessagingDbContext).Assembly.FullName);

        services.AddMessagingServices(configuration);

        return services;
    }
}
