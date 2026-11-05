using EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;
using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Infrastructure.Persistence.Context;
using EnterpriseConnect.RemoteService.Infrastructure.Persistence.Repositories;
using EnterpriseConnect.RemoteService.Infrastructure.Security;
using EnterpriseConnect.RemoteService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseConnect.RemoteService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRemoteInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddPostgresDbContext<RemoteDbContext>(
            configuration,
            connectionStringName: "RemoteDb",
            migrationsAssembly: typeof(RemoteDbContext).Assembly.FullName);

        services.AddScoped<IRemoteSessionRepository, RemoteSessionRepository>();
        services.AddScoped<IAccessGrantRepository, AccessGrantRepository>();
        services.AddScoped<IRemoteUnitOfWork, RemoteUnitOfWork>();

        services.AddSingleton<ISessionCodeGenerator, SessionCodeGenerator>();
        services.AddHostedService<SessionExpirationBackgroundService>();

        return services;
    }
}
