using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Mappings;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionHistory;

public class GetSessionHistoryQueryHandler : IRequestHandler<GetSessionHistoryQuery, Result<IReadOnlyList<RemoteSessionDto>>>
{
    private readonly IRemoteUnitOfWork _unitOfWork;

    public GetSessionHistoryQueryHandler(IRemoteUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<RemoteSessionDto>>> Handle(
        GetSessionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.RemoteSessions.GetUserHistoryAsync(
            request.UserId,
            request.Skip,
            request.Take,
            cancellationToken);

        var dtos = sessions.Select(s => s.ToDto()).ToList();
        return Result<IReadOnlyList<RemoteSessionDto>>.Success(dtos);
    }
}
