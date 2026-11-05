using FluentValidation;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.TerminateSession;

public class TerminateSessionValidator : AbstractValidator<TerminateSessionCommand>
{
    public TerminateSessionValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("SessionId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
