using System.Text;
using EnterpriseConnect.BuildingBlocks.Infrastructure.Extensions;
using EnterpriseConnect.BuildingBlocks.Infrastructure.Security;
using EnterpriseConnect.MediaService.Api.Extensions;
using EnterpriseConnect.MediaService.Api.Hubs;
using EnterpriseConnect.MediaService.Api.Middleware;
using EnterpriseConnect.MediaService.Api.Services;
using EnterpriseConnect.MediaService.Application;
using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Core Services & Layers
builder.Services.AddMediaApplication();
builder.Services.AddMediaInfrastructure(builder.Configuration);
builder.Services.AddEnterpriseHealthChecks();

// 2. JWT Authentication & SignalR Token Support
var jwtKey = builder.Configuration["JwtSettings:Secret"] ?? "REDACTED_JWT_SECRET";
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

// 3. API, CORS & Realtime Hub
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
builder.Services.AddMediaForwardedHeaders();
builder.Services.AddSignalR();
builder.Services.AddScoped<ICallNotificationService, CallNotificationService>();
builder.Services.AddMediaSwagger();

var app = builder.Build();

// 4. Request Pipeline
app.UseForwardedHeaders();
app.UseCors();
app.UseOriginShield();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMediaSwagger(app.Environment);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CallHub>("/hubs/call");
app.MapEnterpriseHealthChecks();

app.MapGet("/", () => Results.Ok("EnterpriseConnect MediaService & CallHub is running..."));

app.Run();

public partial class Program { }
