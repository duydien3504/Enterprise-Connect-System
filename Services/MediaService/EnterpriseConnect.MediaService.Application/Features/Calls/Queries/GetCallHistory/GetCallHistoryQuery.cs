using EnterpriseConnect.MediaService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Queries.GetCallHistory;

public record GetCallHistoryQuery(
    Guid UserId,
    int Skip = 0,
    int Take = 20
) : IRequest<Result<IReadOnlyList<CallSessionDto>>>;
