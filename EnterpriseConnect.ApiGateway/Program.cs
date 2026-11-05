using EnterpriseConnect.ApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Service Registrations via Modules
builder.Services.AddGatewayCors(builder.Configuration);
builder.Services.AddGatewayRateLimiting();
builder.Services.AddGatewayAuth(builder.Configuration);
builder.Services.AddGatewaySwagger();

// 2. YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// 3. Pipeline Execution
app.UseCors(CorsExtensions.PolicyName);
app.UseGatewayCorrelationId();
app.UseInternalEndpointShield();
app.UseGatewaySwagger();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// 4. Endpoints & Proxy Mapping
app.MapGet("/", () => Results.Ok(new
{
    service = "EnterpriseConnect.ApiGateway",
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow,
    engine = "YARP 2.3.0",
    swagger = "/swagger"
}));
app.MapGet("/health", () => Results.Ok("Healthy"));
app.MapReverseProxy();

app.Run();
