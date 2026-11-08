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

                // Hỗ trợ trích xuất token cho SignalR WebSockets và chuẩn hóa header (chống lỗi Bearer lặp từ Swagger UI)
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
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
