using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Mappings;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionByCode;

public class GetSessionByCodeQueryHandler : IRequestHandler<GetSessionByCodeQuery, Result<RemoteSessionDto>>
{
    private readonly IRemoteUnitOfWork _unitOfWork;

    public GetSessionByCodeQueryHandler(IRemoteUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RemoteSessionDto>> Handle(
        GetSessionByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.RemoteSessions.GetByCodeWithGrantsAsync(
            request.SessionCode.Trim(), cancellationToken);

        if (session == null)
        {
            return Result<RemoteSessionDto>.Failure("Session not found.");
        }

        if (session.ExpiresAt.HasValue && session.ExpiresAt.Value < DateTimeOffset.UtcNow)
        {
            session.Status = RemoteSessionStatus.Expired;
            await _unitOfWork.RemoteSessions.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<RemoteSessionDto>.Failure("This session code has expired.");
        }

        return Result<RemoteSessionDto>.Success(session.ToDto());
    }
}
