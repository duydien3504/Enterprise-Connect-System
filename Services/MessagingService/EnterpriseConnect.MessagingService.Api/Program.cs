using System.Text;
using EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;
using EnterpriseConnect.BuildingBlocks.Infrastructure.Security;
using EnterpriseConnect.MessagingService.Api.Extensions;
using EnterpriseConnect.MessagingService.Api.Hubs;
using EnterpriseConnect.MessagingService.Api.Middleware;
using EnterpriseConnect.MessagingService.Application;
using EnterpriseConnect.MessagingService.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Core Services & Layers
builder.Services.AddMessagingApplication();
builder.Services.AddMessagingInfrastructure(builder.Configuration);
builder.Services.AddDistributedAuditClient();
builder.Services.AddEnterpriseHealthChecks();

// 2. JWT Authentication & SignalR Token Support
var rawJwtKey = builder.Configuration["JwtSettings:Secret"]
    ?? Environment.GetEnvironmentVariable("JwtSettings__Secret")
    ?? Environment.GetEnvironmentVariable("JWT_SECRET");

var jwtKey = rawJwtKey?.Trim('"', '\'', ' ');

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("MessagingService: Cấu hình 'JwtSettings:Secret' không được để trống.");
}

var jwtIssuer = (builder.Configuration["JwtSettings:Issuer"] ?? "EnterpriseConnect.IdentityService").Trim('"', '\'', ' ');
var jwtAudience = (builder.Configuration["JwtSettings:Audience"] ?? "EnterpriseConnect.Clients").Trim('"', '\'', ' ');
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "EnterpriseConnect.IdentityService",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "EnterpriseConnect.Clients",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    context.Token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? authHeader.Substring("Bearer ".Length).Trim() : authHeader.Trim();
                }

                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// 3. API & Web Features
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin =>
                  Uri.TryCreate(origin, UriKind.Absolute, out var u) &&
                  (u.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || u.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)))
              .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddMessagingForwardedHeaders();
builder.Services.AddSignalR();
builder.Services.AddMessagingSwagger();

var app = builder.Build();

// 4. Auto-migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EnterpriseConnect.MessagingService.Infrastructure.Persistence.Context.MessagingDbContext>();
    Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.Migrate(dbContext.Database);
}

// 5. Request Pipeline
app.UseForwardedHeaders();
app.UseCors();
app.UseOriginShield();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMessagingSwagger(app.Environment);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapEnterpriseHealthChecks();

app.MapGet("/", () => Results.Ok("MessagingService & SignalR ChatHub is running..."));

app.Run();

public partial class Program { }
