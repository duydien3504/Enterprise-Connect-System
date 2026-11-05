using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record VerifyEmailCommand(string RawToken) : IRequest<bool>;
