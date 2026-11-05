namespace EnterpriseConnect.IdentityService.Application.Features.Auth.DTOs;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; } = 3600; // 60 minutes in seconds
    public string TokenType { get; set; } = "Bearer";
    public UserSummaryDto User { get; set; } = null!;
}
