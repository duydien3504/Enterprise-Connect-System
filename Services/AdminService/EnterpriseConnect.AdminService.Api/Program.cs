using System.Text;
using EnterpriseConnect.AdminService.Api.Extensions;
using EnterpriseConnect.AdminService.Api.Middleware;
using EnterpriseConnect.AdminService.Application;
using EnterpriseConnect.AdminService.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Core Services & Clean Architecture Layers
builder.Services.AddApplication();
builder.Services.AddAdminInfrastructure(builder.Configuration);

// 2. JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:Secret"]
    ?? "REDACTED_JWT_SECRET";

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
                        ? authHeader.Substring("Bearer ".Length).Trim()
                        : authHeader.Trim();
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// 3. API, CORS & Middleware
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin =>
                  Uri.TryCreate(origin, UriKind.Absolute, out var u) &&
                  (u.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || u.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)))
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddAdminForwardedHeaders();
builder.Services.AddAdminSwagger();

var app = builder.Build();

// 4. Auto-migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EnterpriseConnect.AdminService.Infrastructure.Persistence.Context.AdminDbContext>();
    Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.Migrate(dbContext.Database);
}

// 5. Request Pipeline
app.UseForwardedHeaders();
app.UseCors();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAdminSwagger(app.Environment);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Ok("EnterpriseConnect AdminService is running..."));

app.Run();

public partial class Program { }
