using EnterpriseConnect.IdentityService.Domain.Enums;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.DTOs;

public record UserSessionDto(
    Guid Id,
    DeviceType DeviceType,
    string? DeviceInfo,
    string? IpAddress,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    bool IsRevoked
);
