using EnterpriseConnect.AdminService.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAdminInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EnterpriseConnect Admin API",
        Version = "v1",
        Description = "Admin API for EnterpriseConnect"
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EnterpriseConnect Admin API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok("AdminService is running..."));

app.Run();
