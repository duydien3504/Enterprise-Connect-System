using EnterpriseConnect.MediaService.Api.Hubs;
using EnterpriseConnect.MediaService.Api.Services;
using EnterpriseConnect.MediaService.Application;
using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Clean Architecture Layers
builder.Services.AddMediaApplication();
builder.Services.AddMediaInfrastructure(builder.Configuration);

// SignalR Realtime & Notifications
builder.Services.AddSignalR();
builder.Services.AddScoped<ICallNotificationService, CallNotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHub<CallHub>("/hubs/call");

app.MapGet("/", () => Results.Ok("MediaService is running..."));

app.Run();
