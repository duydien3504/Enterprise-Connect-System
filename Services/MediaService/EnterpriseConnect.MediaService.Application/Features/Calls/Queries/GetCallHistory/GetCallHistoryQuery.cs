using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Common.Mappings;
using EnterpriseConnect.MediaService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Queries.GetCallHistory;

public record GetCallHistoryQuery(Guid UserId, int Skip = 0, int Take = 20) : IRequest<Result<IReadOnlyList<CallSessionDto>>>;

public class GetCallHistoryQueryHandler : IRequestHandler<GetCallHistoryQuery, Result<IReadOnlyList<CallSessionDto>>>
{
    private readonly IMediaUnitOfWork _unitOfWork;

    public GetCallHistoryQueryHandler(IMediaUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<CallSessionDto>>> Handle(GetCallHistoryQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _unitOfWork.CallSessions.GetUserCallHistoryAsync(request.UserId, request.Skip, request.Take, cancellationToken);
        var dtos = sessions.Select(s => s.ToDto()).ToList();
        return Result<IReadOnlyList<CallSessionDto>>.Success(dtos);
    }
}
