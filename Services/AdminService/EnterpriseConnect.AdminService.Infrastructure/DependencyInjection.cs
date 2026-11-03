using EnterpriseConnect.AdminService.Infrastructure.Persistence.Context;
using EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseConnect.AdminService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPostgresDbContext<AdminDbContext>(
            configuration,
            connectionStringName: "AdminDb",
            migrationsAssembly: typeof(AdminDbContext).Assembly.FullName);

        return services;
    }
}
