using EnterpriseConnect.IdentityService.Application.Features.Auth.DTOs;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Queries;

public record GetUserSessionsQuery(Guid UserId) : IRequest<IReadOnlyList<UserSessionDto>>;
