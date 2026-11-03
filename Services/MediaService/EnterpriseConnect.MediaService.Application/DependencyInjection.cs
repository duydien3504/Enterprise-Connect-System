using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseConnect.MediaService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMediaApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
