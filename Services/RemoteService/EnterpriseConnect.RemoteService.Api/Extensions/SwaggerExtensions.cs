using Microsoft.OpenApi.Models;

namespace EnterpriseConnect.RemoteService.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddRemoteSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EnterpriseConnect Remote Assistance API",
                Version = "v1",
                Description = "Remote Desktop Assistance API for EnterpriseConnect"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Dán trực tiếp chuỗi token (không cần gõ chữ Bearer).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseRemoteSwagger(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        app.UseSwagger(c =>
        {
            c.SerializeAsV2 = true;
            c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            {
                if (httpReq.Headers.TryGetValue("X-Forwarded-Prefix", out var prefix) && !string.IsNullOrEmpty(prefix))
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new() { Url = prefix.ToString() }
                    };
                }
            });
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "EnterpriseConnect Remote Assistance API v1");
            c.RoutePrefix = "swagger";
        });

        return app;
    }
}
