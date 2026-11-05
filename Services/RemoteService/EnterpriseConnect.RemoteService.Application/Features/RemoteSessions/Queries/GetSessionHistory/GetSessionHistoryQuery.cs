using EnterpriseConnect.RemoteService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionHistory;

public record GetSessionHistoryQuery(
    Guid UserId,
    int Skip = 0,
    int Take = 20
) : IRequest<Result<IReadOnlyList<RemoteSessionDto>>>;
