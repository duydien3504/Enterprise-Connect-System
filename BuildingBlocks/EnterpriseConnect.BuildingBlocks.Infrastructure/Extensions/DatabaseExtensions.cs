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
        var rawConn = configuration.GetConnectionString(connectionStringName)
            ?? configuration[$"ConnectionStrings:{connectionStringName}"]
            ?? configuration[connectionStringName]
            ?? configuration["DATABASE_URL"]
            ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

        var connectionString = NormalizePostgresConnectionString(rawConn);

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

    private static string NormalizePostgresConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
            connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
            var database = uri.AbsolutePath.TrimStart('/');
            var port = uri.Port > 0 ? uri.Port : 5432;

            return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};Include Error Detail=true;";
        }

        return connectionString;
    }
}
