using EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Queries;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileDto>;
