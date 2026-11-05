using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record LogoutAllDevicesCommand(Guid UserId) : IRequest<bool>;
