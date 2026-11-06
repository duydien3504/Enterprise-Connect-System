namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    Guid? DepartmentId { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<Guid> RoleIds { get; }
    IReadOnlyList<string> Permissions { get; }
}
