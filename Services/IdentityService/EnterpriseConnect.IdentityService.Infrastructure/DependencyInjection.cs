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

        // Extended Services: Caching, Email, Cloudinary & Image Validation
        var redisConn = configuration.GetConnectionString("Redis") ?? configuration["RedisSettings:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConn))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConn;
                options.InstanceName = "EnterpriseConnect:";
            });
        }
        else
        {
            // Fallback in-memory distributed cache khi chưa bật Redis server
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, DistributedCacheService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<ICloudinaryStorageService, CloudinaryStorageService>();
        services.AddScoped<IImageValidator, ImageSharpValidator>();
        services.AddSingleton<IPublicUrlProvider, PublicUrlProvider>();

        // JWT Authentication Configuration
        var secretKey = configuration["JwtSettings:Secret"]
            ?? Environment.GetEnvironmentVariable("JwtSettings__Secret")
            ?? Environment.GetEnvironmentVariable("JWT_SECRET");

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("IdentityService: Cấu hình bảo mật 'JwtSettings:Secret' không được để trống. Vui lòng thiết lập biến môi trường JwtSettings__Secret.");
        }

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

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hubs") || path.StartsWithSegments("/identity/hubs")))
                    {
                        context.Token = accessToken;
                        return Task.CompletedTask;
                    }

                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        var raw = authHeader.Trim();
                        while (raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            raw = raw.Substring(7).Trim();
                        }
                        context.Token = raw;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
