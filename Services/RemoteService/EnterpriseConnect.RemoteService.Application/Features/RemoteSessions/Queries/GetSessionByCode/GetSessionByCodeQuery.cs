using EnterpriseConnect.RemoteService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionByCode;

public record GetSessionByCodeQuery(
    string SessionCode
) : IRequest<Result<RemoteSessionDto>>;
