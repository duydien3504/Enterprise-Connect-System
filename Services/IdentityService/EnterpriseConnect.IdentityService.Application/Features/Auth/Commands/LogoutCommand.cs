using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken) : IRequest<bool>;
