using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Queries.GetViolations;

public class GetViolationsValidator : AbstractValidator<GetViolationsQuery>
{
    public GetViolationsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
