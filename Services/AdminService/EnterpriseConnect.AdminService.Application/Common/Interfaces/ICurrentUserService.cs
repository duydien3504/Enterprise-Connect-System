namespace EnterpriseConnect.AdminService.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
