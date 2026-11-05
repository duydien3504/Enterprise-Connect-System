using EnterpriseConnect.RemoteService.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRemoteInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EnterpriseConnect Remote API",
        Version = "v1",
        Description = "Remote API for EnterpriseConnect"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = true;
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EnterpriseConnect Remote API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok("RemoteService is running..."));

app.Run();
