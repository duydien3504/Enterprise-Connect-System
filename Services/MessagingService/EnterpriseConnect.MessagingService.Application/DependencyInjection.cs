using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseConnect.MessagingService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMessagingApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}
