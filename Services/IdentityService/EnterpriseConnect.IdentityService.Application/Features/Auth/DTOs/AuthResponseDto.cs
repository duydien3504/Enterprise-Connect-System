namespace EnterpriseConnect.IdentityService.Application.Features.Auth.DTOs;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; } = 3600; // 60 minutes in seconds
    public string TokenType { get; set; } = "Bearer";
    public UserSummaryDto User { get; set; } = null!;
}

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = new List<string>();
    public IReadOnlyList<string> Permissions { get; set; } = new List<string>();
}
