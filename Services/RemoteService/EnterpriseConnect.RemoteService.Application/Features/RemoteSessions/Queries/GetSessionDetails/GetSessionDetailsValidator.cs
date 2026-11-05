using FluentValidation;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionDetails;

public class GetSessionDetailsValidator : AbstractValidator<GetSessionDetailsQuery>
{
    public GetSessionDetailsValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("SessionId is required.");
    }
}
