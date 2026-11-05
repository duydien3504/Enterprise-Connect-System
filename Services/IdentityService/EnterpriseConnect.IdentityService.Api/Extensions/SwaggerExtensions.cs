using Microsoft.OpenApi.Models;

namespace EnterpriseConnect.IdentityService.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddIdentitySwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EnterpriseConnect Identity API",
                Version = "v1",
                Description = "Authentication & Authorization API for EnterpriseConnect"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter ONLY your token in the text input below (Bearer prefix will be added automatically).",
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

    public static IApplicationBuilder UseIdentitySwagger(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityService API v1");
                c.RoutePrefix = "swagger";
            });
        }

        return app;
    }
}
