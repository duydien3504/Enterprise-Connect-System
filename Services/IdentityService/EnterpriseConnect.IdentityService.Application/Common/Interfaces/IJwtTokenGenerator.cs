using EnterpriseConnect.IdentityService.Domain.Entities;

namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
}
