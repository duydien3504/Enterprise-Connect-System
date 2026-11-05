using FluentValidation;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.ApproveSession;

public class ApproveSessionValidator : AbstractValidator<ApproveSessionCommand>
{
    public ApproveSessionValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("SessionId is required.");

        RuleFor(x => x.RequesterId)
            .NotEmpty().WithMessage("RequesterId is required.");
    }
}
