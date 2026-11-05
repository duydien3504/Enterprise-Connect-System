using EnterpriseConnect.IdentityService.Application.Features.Auth.DTOs;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password,
    DeviceType DeviceType = DeviceType.Web,
    string? DeviceInfo = null,
    string? IpAddress = null
) : IRequest<AuthResponseDto>;
