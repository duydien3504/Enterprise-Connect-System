using FluentValidation;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionByCode;

public class GetSessionByCodeValidator : AbstractValidator<GetSessionByCodeQuery>
{
    public GetSessionByCodeValidator()
    {
        RuleFor(x => x.SessionCode)
            .NotEmpty().WithMessage("SessionCode is required.")
            .MaximumLength(20).WithMessage("SessionCode cannot exceed 20 characters.");
    }
}
