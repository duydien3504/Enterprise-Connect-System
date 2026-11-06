using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Queries.GetViolationById;

public class GetViolationByIdValidator : AbstractValidator<GetViolationByIdQuery>
{
    public GetViolationByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
