namespace EnterpriseConnect.ApiGateway.Extensions;

using EnterpriseConnect.ApiGateway.Services;

public static class SwaggerAggregatorExtensions
{
    public static IServiceCollection AddGatewaySwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddHttpClient("SwaggerAggregator", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
        services.AddSingleton<UnifiedSwaggerService>();
        return services;
    }

    public static IApplicationBuilder UseGatewaySwagger(this IApplicationBuilder app)
    {
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "EnterpriseConnect API Gateway - Swagger UI";

            // 1. Unified spec containing all endpoints (Conversations, Messages, Auth, Users, etc.)
            options.SwaggerEndpoint("/swagger/unified/swagger.json", "★ Tat ca Services (Unified All)");

            // 2. Individual Microservice Specs
            options.SwaggerEndpoint("/messaging/swagger/v1/swagger.json", "Messaging Service API v1");
            options.SwaggerEndpoint("/identity/swagger/v1/swagger.json", "Identity Service API v1");
            options.SwaggerEndpoint("/admin/swagger/v1/swagger.json", "Admin Service API v1");
            options.SwaggerEndpoint("/media/swagger/v1/swagger.json", "Media Service API v1");
            options.SwaggerEndpoint("/remote/swagger/v1/swagger.json", "Remote Service API v1");
        });

        return app;
    }
}
