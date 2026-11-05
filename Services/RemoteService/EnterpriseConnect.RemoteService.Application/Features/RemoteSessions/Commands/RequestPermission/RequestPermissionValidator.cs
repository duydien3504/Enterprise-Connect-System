using FluentValidation;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RequestPermission;

public class RequestPermissionValidator : AbstractValidator<RequestPermissionCommand>
{
    public RequestPermissionValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("SessionId is required.");

        RuleFor(x => x.SupporterId)
            .NotEmpty().WithMessage("SupporterId is required.");

        RuleFor(x => x.PermissionType)
            .IsInEnum().WithMessage("Valid PermissionType is required.");
    }
}
