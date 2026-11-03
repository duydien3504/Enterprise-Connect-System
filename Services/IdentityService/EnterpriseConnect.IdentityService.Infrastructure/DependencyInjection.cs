using System.Text;
using EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Infrastructure.Persistence;
using EnterpriseConnect.IdentityService.Infrastructure.Persistence.Context;
using EnterpriseConnect.IdentityService.Infrastructure.Persistence.Repositories;
using EnterpriseConnect.IdentityService.Infrastructure.Security;
using EnterpriseConnect.IdentityService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseConnect.IdentityService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL DbContext
        services.AddPostgresDbContext<IdentityDbContext>(
            configuration,
            connectionStringName: "IdentityDb",
            migrationsAssembly: typeof(IdentityDbContext).Assembly.FullName);

        // Register Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        // Security & Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IPresenceService, MemoryPresenceService>();
        services.AddHttpContextAccessor();

        // JWT Authentication Configuration
        var secretKey = configuration["JwtSettings:Secret"] ?? "REDACTED_JWT_SECRET";
        var issuer = configuration["JwtSettings:Issuer"] ?? "EnterpriseConnect.IdentityService";
        var audience = configuration["JwtSettings:Audience"] ?? "EnterpriseConnect.Clients";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}
