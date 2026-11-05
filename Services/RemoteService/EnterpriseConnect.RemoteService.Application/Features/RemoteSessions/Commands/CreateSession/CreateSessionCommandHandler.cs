using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Mappings;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.CreateSession;

public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, Result<RemoteSessionDto>>
{
    private readonly IRemoteUnitOfWork _unitOfWork;
    private readonly ISessionCodeGenerator _codeGenerator;

    public CreateSessionCommandHandler(
        IRemoteUnitOfWork unitOfWork,
        ISessionCodeGenerator codeGenerator)
    {
        _unitOfWork = unitOfWork;
        _codeGenerator = codeGenerator;
    }

    public async Task<Result<RemoteSessionDto>> Handle(
        CreateSessionCommand request,
        CancellationToken cancellationToken)
    {
        var activeSession = await _unitOfWork.RemoteSessions.GetActiveSessionByRequesterAsync(
            request.RequesterId, cancellationToken);

        if (activeSession != null)
        {
            activeSession.Status = RemoteSessionStatus.Terminated;
            activeSession.EndedAt = DateTimeOffset.UtcNow;
            activeSession.EndReason = "Superseded by new session creation";
            await _unitOfWork.RemoteSessions.UpdateAsync(activeSession, cancellationToken);
        }

        var session = new RemoteSession
        {
            Id = Guid.NewGuid(),
            SessionCode = _codeGenerator.GenerateCode(),
            RequesterId = request.RequesterId,
            Status = RemoteSessionStatus.Created,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(request.ExpirationMinutes),
            IsSingleUse = true
        };

        session.AccessGrants.Add(new AccessGrant
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            PermissionType = PermissionType.ViewScreen,
            IsGranted = false
        });

        session.AccessGrants.Add(new AccessGrant
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            PermissionType = PermissionType.ControlInput,
            IsGranted = false
        });

        await _unitOfWork.RemoteSessions.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RemoteSessionDto>.Success(session.ToDto());
    }
}
