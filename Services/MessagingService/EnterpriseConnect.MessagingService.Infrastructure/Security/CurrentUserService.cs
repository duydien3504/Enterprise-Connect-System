using System.Security.Claims;
using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EnterpriseConnect.MessagingService.Infrastructure.Security;

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

    public string? FullName =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("name");

    public Guid? DepartmentId
    {
        get
        {
            var dept = _httpContextAccessor.HttpContext?.User.FindFirstValue("department_id")
                    ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("DepartmentId")
                    ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Department-Id"].FirstOrDefault();

            return Guid.TryParse(dept, out var id) ? id : null;
        }
    }

    public IReadOnlyList<string> Roles =>
        _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Union(_httpContextAccessor.HttpContext?.User.FindAll("role").Select(c => c.Value) ?? Enumerable.Empty<string>())
            .Distinct()
            .ToList() ?? new List<string>();

    public IReadOnlyList<Guid> RoleIds
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var headers = _httpContextAccessor.HttpContext?.Request.Headers;
            var headerList = headers != null && headers.TryGetValue("X-Role-Ids", out var val)
                ? val.SelectMany(h => (h ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                : Enumerable.Empty<string>();

            var claims = (user?.FindAll("role_id").Select(c => c.Value) ?? Enumerable.Empty<string>())
                .Union(user?.FindAll("roleId").Select(c => c.Value) ?? Enumerable.Empty<string>())
                .Union(user?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>())
                .Union(headerList)
                .Distinct();

            var list = new List<Guid>();
            foreach (var c in claims)
            {
                if (Guid.TryParse(c, out var roleId))
                {
                    list.Add(roleId);
                }
            }
            return list;
        }
    }

    public IReadOnlyList<string> Permissions =>
        _httpContextAccessor.HttpContext?.User.FindAll("permission").Select(c => c.Value).Distinct().ToList()
        ?? new List<string>();
}
