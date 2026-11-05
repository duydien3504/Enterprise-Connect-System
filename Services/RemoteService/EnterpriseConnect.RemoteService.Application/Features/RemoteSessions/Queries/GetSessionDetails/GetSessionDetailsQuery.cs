using EnterpriseConnect.RemoteService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionDetails;

public record GetSessionDetailsQuery(
    Guid SessionId
) : IRequest<Result<RemoteSessionDto>>;
