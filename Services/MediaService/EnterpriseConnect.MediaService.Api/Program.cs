using EnterpriseConnect.MediaService.Api.Extensions;
using EnterpriseConnect.MediaService.Api.Hubs;
using EnterpriseConnect.MediaService.Api.Services;
using EnterpriseConnect.MediaService.Application;
using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMediaForwardedHeaders();
builder.Services.AddMediaSwagger();

// Clean Architecture Layers
builder.Services.AddMediaApplication();
builder.Services.AddMediaInfrastructure(builder.Configuration);

// SignalR Realtime & Notifications
builder.Services.AddSignalR();
builder.Services.AddScoped<ICallNotificationService, CallNotificationService>();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseMediaSwagger(app.Environment);

app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<CallHub>("/hubs/call");

app.MapGet("/", () => Results.Ok("MediaService is running..."));

app.Run();
