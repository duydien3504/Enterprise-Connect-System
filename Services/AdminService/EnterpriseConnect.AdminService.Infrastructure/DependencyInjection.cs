using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Services;
using EnterpriseConnect.AdminService.Infrastructure.Persistence.Context;
using EnterpriseConnect.AdminService.Infrastructure.Persistence.Repositories;
using EnterpriseConnect.AdminService.Infrastructure.Security;
using EnterpriseConnect.AdminService.Infrastructure.Services;
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

        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IToxicViolationRepository, ToxicViolationRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<IAdminUnitOfWork, AdminUnitOfWork>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IAuditLogDeduplicator, AuditLogDeduplicator>();
        services.AddScoped<ICloudinaryStorageService, CloudinaryStorageService>();
        services.AddSingleton<IDocumentChunker, DocumentChunker>();
        services.AddHttpClient<IKnowledgeSyncClient, HttpKnowledgeSyncClient>();

        return services;
    }
}
