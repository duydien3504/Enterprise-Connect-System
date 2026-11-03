using System.Security.Claims;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EnterpriseConnect.IdentityService.Infrastructure.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub")
                   ?? _httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("email");

    public IReadOnlyList<string> Roles =>
        _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Union(_httpContextAccessor.HttpContext?.User.FindAll("role").Select(c => c.Value) ?? Enumerable.Empty<string>())
            .Distinct()
            .ToList() ?? new List<string>();

    public IReadOnlyList<string> Permissions =>
        _httpContextAccessor.HttpContext?.User.FindAll("permission").Select(c => c.Value).Distinct().ToList()
        ?? new List<string>();

    public bool HasPermission(string permission) => Permissions.Contains(permission);
}
