using FluentValidation;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.CreateSession;

public class CreateSessionValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionValidator()
    {
        RuleFor(x => x.RequesterId)
            .NotEmpty().WithMessage("RequesterId is required.");

        RuleFor(x => x.ExpirationMinutes)
            .InclusiveBetween(1, 120).WithMessage("ExpirationMinutes must be between 1 and 120.");
    }
}
