namespace EnterpriseConnect.IdentityService.Application.Features.Auth.DTOs;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = new List<string>();
    public IReadOnlyList<string> Permissions { get; set; } = new List<string>();
}
