using FluentValidation;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RevokePermission;

public class RevokePermissionValidator : AbstractValidator<RevokePermissionCommand>
{
    public RevokePermissionValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("SessionId is required.");

        RuleFor(x => x.RequesterId)
            .NotEmpty().WithMessage("RequesterId is required.");

        RuleFor(x => x.PermissionType)
            .IsInEnum().WithMessage("Valid PermissionType is required.");
    }
}
