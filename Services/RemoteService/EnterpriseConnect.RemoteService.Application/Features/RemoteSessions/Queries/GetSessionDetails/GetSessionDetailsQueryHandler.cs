using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Mappings;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionDetails;

public class GetSessionDetailsQueryHandler : IRequestHandler<GetSessionDetailsQuery, Result<RemoteSessionDto>>
{
    private readonly IRemoteUnitOfWork _unitOfWork;

    public GetSessionDetailsQueryHandler(IRemoteUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RemoteSessionDto>> Handle(
        GetSessionDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.RemoteSessions.GetByIdWithGrantsAsync(
            request.SessionId, cancellationToken);

        if (session == null)
        {
            return Result<RemoteSessionDto>.Failure("Session not found.");
        }

        return Result<RemoteSessionDto>.Success(session.ToDto());
    }
}
