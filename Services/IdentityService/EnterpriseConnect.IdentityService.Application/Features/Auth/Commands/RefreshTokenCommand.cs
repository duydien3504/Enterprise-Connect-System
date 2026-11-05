using EnterpriseConnect.IdentityService.Application.Features.Auth.DTOs;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress = null,
    string? DeviceInfo = null
) : IRequest<AuthResponseDto>;
