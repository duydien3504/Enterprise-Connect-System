using EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;
using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Infrastructure.Persistence.Context;
using EnterpriseConnect.MediaService.Infrastructure.Persistence.Repositories;
using EnterpriseConnect.MediaService.Infrastructure.Services;
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

        services.AddScoped<IMediaUnitOfWork, MediaUnitOfWork>();
        services.AddSingleton<ILiveKitTokenService, LiveKitTokenService>();
        services.AddSingleton<IWebRtcIceService, WebRtcIceService>();

        return services;
    }
}
