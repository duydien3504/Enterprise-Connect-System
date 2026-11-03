using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Queries;

public record UserSessionDto(
    Guid Id,
    DeviceType DeviceType,
    string? DeviceInfo,
    string? IpAddress,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    bool IsRevoked
);

public record GetUserSessionsQuery(Guid UserId) : IRequest<IReadOnlyList<UserSessionDto>>;

public class GetUserSessionsQueryHandler : IRequestHandler<GetUserSessionsQuery, IReadOnlyList<UserSessionDto>>
{
    private readonly IIdentityUnitOfWork _uow;

    public GetUserSessionsQueryHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<UserSessionDto>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _uow.UserSessions.FindAsync(
            s => s.UserId == request.UserId && !s.IsRevoked && s.ExpiresAt > DateTimeOffset.UtcNow,
            cancellationToken);

        return sessions.Select(s => new UserSessionDto(
            s.Id,
            s.DeviceType,
            s.DeviceInfo,
            s.IpAddress,
            s.CreatedAt,
            s.ExpiresAt,
            s.IsRevoked
        )).OrderByDescending(s => s.CreatedAt).ToList();
    }
}
