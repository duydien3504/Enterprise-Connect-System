using EnterpriseConnect.MessagingService.Api.Extensions;
using EnterpriseConnect.MessagingService.Api.Hubs;
using EnterpriseConnect.MessagingService.Application;
using EnterpriseConnect.MessagingService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMessagingApplication();
builder.Services.AddMessagingInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddMessagingForwardedHeaders();
builder.Services.AddSignalR();
builder.Services.AddMessagingSwagger();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseMessagingSwagger(app.Environment);

app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.MapGet("/", () => Results.Ok("MessagingService is running..."));

app.Run();
