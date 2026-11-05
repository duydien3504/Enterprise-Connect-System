using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseConnect.ApiGateway.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddGatewayAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["JwtSettings:Secret"] ?? "REDACTED_JWT_SECRET";
        var issuer = configuration["JwtSettings:Issuer"] ?? "EnterpriseConnect.IdentityService";
        var audience = configuration["JwtSettings:Audience"] ?? "EnterpriseConnect.Clients";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                // Hỗ trợ trích xuất token cho SignalR WebSockets và Swagger UI tự động nhận cả Bearer lẫn token trần
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        else
                        {
                            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                            if (!string.IsNullOrEmpty(authHeader) && !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = authHeader.Trim();
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}
