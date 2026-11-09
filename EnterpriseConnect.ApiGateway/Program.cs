using EnterpriseConnect.ApiGateway.Extensions;
using EnterpriseConnect.ApiGateway.Services;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel & Form limits for file uploads / proxying (25MB)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 25 * 1024 * 1024;
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 25 * 1024 * 1024;
});

// 1. Service Registrations via Modules
builder.Services.AddGatewayCors(builder.Configuration);
builder.Services.AddGatewayRateLimiting();
builder.Services.AddGatewayAuth(builder.Configuration);
builder.Services.AddGatewaySwagger();

// 2. YARP Reverse Proxy with Origin Shield Secret Injection
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        const string masterSecret = "EnterpriseConnect_Gateway_Secret_2026_Prod!";
        var rawSecret = builder.Configuration["Security:GatewaySharedSecret"];
        var envSecret = Environment.GetEnvironmentVariable("GATEWAY_SHARED_SECRET")
                     ?? Environment.GetEnvironmentVariable("Security__GatewaySharedSecret");

        var secret = (!string.IsNullOrWhiteSpace(rawSecret) ? rawSecret.Trim('"', '\'', ' ') : null)
                  ?? (!string.IsNullOrWhiteSpace(envSecret) ? envSecret.Trim('"', '\'', ' ') : null)
                  ?? masterSecret;

        builderContext.AddRequestTransform(transformContext =>
        {
            transformContext.ProxyRequest.Headers.Remove("X-Gateway-Secret");
            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-Gateway-Secret", secret);
            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

// 3. Pipeline Execution
app.UseCors(CorsExtensions.PolicyName);
app.UseGatewaySecurityHeaders();
app.UseGatewayCorrelationId();
app.UseInternalEndpointShield();
app.UseGatewaySwagger();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// 4. Endpoints & Proxy Mapping
app.MapGet("/", () => Results.Ok(new
{
    service = "EnterpriseConnect.ApiGateway", status = "Healthy",
    timestamp = DateTimeOffset.UtcNow, engine = "YARP 2.3.0", swagger = "/swagger"
}));
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTimeOffset.UtcNow }));
app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy", timestamp = DateTimeOffset.UtcNow }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "Healthy", timestamp = DateTimeOffset.UtcNow }));
app.MapGet("/swagger/unified/swagger.json", async (UnifiedSwaggerService service, CancellationToken ct) =>
    Results.Content(await service.GetUnifiedSwaggerJsonAsync(ct), "application/json"));
app.MapReverseProxy();

app.Run();
