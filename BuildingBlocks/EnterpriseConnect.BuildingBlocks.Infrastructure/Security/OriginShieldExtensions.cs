using Microsoft.AspNetCore.Builder;

namespace EnterpriseConnect.BuildingBlocks.Infrastructure.Security;

public static class OriginShieldExtensions
{
    public static IApplicationBuilder UseOriginShield(this IApplicationBuilder app)
    {
        return app.UseMiddleware<OriginShieldMiddleware>();
    }
}
