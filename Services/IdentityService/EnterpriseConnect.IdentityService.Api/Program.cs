using EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;
using EnterpriseConnect.BuildingBlocks.Infrastructure.Security;
using EnterpriseConnect.IdentityService.Api.Authorization;
using EnterpriseConnect.IdentityService.Api.Extensions;
using EnterpriseConnect.IdentityService.Api.Middleware;
using EnterpriseConnect.IdentityService.Application;
using EnterpriseConnect.IdentityService.Infrastructure;
using EnterpriseConnect.IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel & Form limits for file uploads (25MB)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 25 * 1024 * 1024;
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 25 * 1024 * 1024;
});

// 1. Core & Architecture Services
builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddDistributedAuditClient();
builder.Services.AddEnterpriseHealthChecks();
builder.Services.AddIdentityForwardedHeaders(builder.Configuration);
builder.Services.AddIdentityCors();

// 2. Custom Authorization Policies & Swagger
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddControllers();
builder.Services.AddIdentitySwagger();

var app = builder.Build();

// 3. Database Migration & Seed Data
await IdentityDbSeeder.SeedAsync(app.Services);
if (args.Contains("--seed-only")) return;

// 4. Request Pipeline
app.UseForwardedHeaders();
app.UseCors(CorsExtensions.PolicyName);
app.UseIdentitySwagger(app.Environment);
app.UseOriginShield();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapEnterpriseHealthChecks();
app.MapGet("/", () => Results.Ok("IdentityService Auth & Authorization is active!"));

app.Run();

public partial class Program { }
