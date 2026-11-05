using FluentValidation;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.JoinSession;

public class JoinSessionValidator : AbstractValidator<JoinSessionCommand>
{
    public JoinSessionValidator()
    {
        RuleFor(x => x.SessionCode)
            .NotEmpty().WithMessage("SessionCode is required.")
            .MaximumLength(20).WithMessage("SessionCode cannot exceed 20 characters.");

        RuleFor(x => x.SupporterId)
            .NotEmpty().WithMessage("SupporterId is required.");
    }
}
