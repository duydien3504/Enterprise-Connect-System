using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RequestPermission;

public class RequestPermissionCommandHandler : IRequestHandler<RequestPermissionCommand, Result<bool>>
{
    private readonly IRemoteUnitOfWork _unitOfWork;
    private readonly IRemoteNotificationService _notificationService;

    public RequestPermissionCommandHandler(
        IRemoteUnitOfWork unitOfWork,
        IRemoteNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> Handle(
        RequestPermissionCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.RemoteSessions.GetByIdAsync(
            request.SessionId, cancellationToken);

        if (session == null)
        {
            return Result<bool>.Failure("Session not found.");
        }

        if (session.Status != RemoteSessionStatus.Active)
        {
            return Result<bool>.Failure("Cannot request permissions on an inactive session.");
        }

        if (session.SupporterId != request.SupporterId)
        {
            return Result<bool>.Failure("Only the assigned supporter can request permissions.");
        }

        await _notificationService.NotifyPermissionRequestedAsync(
            session.RequesterId, request.PermissionType);

        return Result<bool>.Success(true);
    }
}
