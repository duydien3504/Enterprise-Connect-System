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

        // BOLA / IDOR Prevention: Bắt buộc người truy vấn phải là Requester hoặc Supporter của phiên
        if (request.CurrentUserId.HasValue && request.CurrentUserId.Value != Guid.Empty)
        {
            var userId = request.CurrentUserId.Value;
            var isParticipant = session.RequesterId == userId || session.SupporterId == userId;
            if (!isParticipant)
            {
                return Result<RemoteSessionDto>.Failure("Access denied: You are not a participant of this remote session.");
            }
        }

        return Result<RemoteSessionDto>.Success(session.ToDto());
    }
}
