using FluentValidation;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Queries.GetCallHistory;

public class GetCallHistoryValidator : AbstractValidator<GetCallHistoryQuery>
{
    public GetCallHistoryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0.");

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 100).WithMessage("Take must be between 1 and 100.");
    }
}
