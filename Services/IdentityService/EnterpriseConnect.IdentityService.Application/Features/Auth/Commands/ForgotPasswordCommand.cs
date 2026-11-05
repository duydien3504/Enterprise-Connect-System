using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<bool>;
