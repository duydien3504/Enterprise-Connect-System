using EnterpriseConnect.BuildingBlocks.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddPostgresDbContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName,
        string? migrationsAssembly = null) where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                if (!string.IsNullOrEmpty(migrationsAssembly))
                {
                    npgsqlOptions.MigrationsAssembly(migrationsAssembly);
                }
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            // PostgreSQL snake_case naming convention
            options.UseSnakeCaseNamingConvention();
        });

        // Register DbContext as base DbContext for repository injection
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());

        // Register UnitOfWork if context implements it
        if (typeof(IUnitOfWork).IsAssignableFrom(typeof(TContext)))
        {
            services.AddScoped<IUnitOfWork>(sp => (IUnitOfWork)sp.GetRequiredService<TContext>());
        }

        // Register Generic EF Repository
        services.AddScoped(typeof(IRepository<>), typeof(Persistence.EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(Persistence.EfRepository<>));

        return services;
    }
}
