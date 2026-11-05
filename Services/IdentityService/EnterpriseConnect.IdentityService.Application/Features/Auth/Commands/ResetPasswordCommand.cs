using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record ResetPasswordCommand(string Email, string Otp, string NewPassword) : IRequest<bool>;
