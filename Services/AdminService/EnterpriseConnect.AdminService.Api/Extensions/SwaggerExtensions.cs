using Microsoft.OpenApi.Models;

namespace EnterpriseConnect.AdminService.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddAdminSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EnterpriseConnect Administration API",
                Version = "v1",
                Description = "Administration & Cyber Forensics API for EnterpriseConnect"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Dán trực tiếp chuỗi token (không cần gõ chữ Bearer).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
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

    public static IApplicationBuilder UseAdminSwagger(this IApplicationBuilder app, IWebHostEnvironment environment)
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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "EnterpriseConnect Administration API v1");
            c.RoutePrefix = "swagger";
        });

        return app;
    }
}
