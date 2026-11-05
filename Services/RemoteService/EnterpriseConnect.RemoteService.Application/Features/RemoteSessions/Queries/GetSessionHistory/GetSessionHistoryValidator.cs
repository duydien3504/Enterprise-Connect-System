using FluentValidation;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionHistory;

public class GetSessionHistoryValidator : AbstractValidator<GetSessionHistoryQuery>
{
    public GetSessionHistoryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0.");

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 100).WithMessage("Take must be between 1 and 100.");
    }
}
