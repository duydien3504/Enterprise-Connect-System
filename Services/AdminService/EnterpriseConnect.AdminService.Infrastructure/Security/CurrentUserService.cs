using System.Security.Claims;
using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EnterpriseConnect.AdminService.Infrastructure.Security;

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

    public string? Role =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("role")
        ?? _httpContextAccessor.HttpContext?.Request.Headers["X-User-Role"].FirstOrDefault();

    public string? IpAddress
    {
        get
        {
            var forwarded = _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                return forwarded.Split(',')[0].Trim();
            }
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        }
    }

    public string? UserAgent =>
        _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
}
