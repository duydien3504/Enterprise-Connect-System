using FluentValidation;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RespondPermission;

public class RespondPermissionValidator : AbstractValidator<RespondPermissionCommand>
{
    public RespondPermissionValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("SessionId is required.");

        RuleFor(x => x.RequesterId)
            .NotEmpty().WithMessage("RequesterId is required.");

        RuleFor(x => x.PermissionType)
            .IsInEnum().WithMessage("Valid PermissionType is required.");
    }
}
