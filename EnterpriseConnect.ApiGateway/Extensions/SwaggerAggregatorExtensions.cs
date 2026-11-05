namespace EnterpriseConnect.ApiGateway.Extensions;

public static class SwaggerAggregatorExtensions
{
    public static IServiceCollection AddGatewaySwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        return services;
    }

    public static IApplicationBuilder UseGatewaySwagger(this IApplicationBuilder app)
    {
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "EnterpriseConnect API Gateway - Swagger UI";

            // Aggregated Endpoints for Microservices via Gateway Proxy
            options.SwaggerEndpoint("/identity/swagger/v1/swagger.json", "Identity Service API v1");
            options.SwaggerEndpoint("/messaging/swagger/v1/swagger.json", "Messaging Service API v1");
            options.SwaggerEndpoint("/media/swagger/v1/swagger.json", "Media Service API v1");
        });

        return app;
    }
}
