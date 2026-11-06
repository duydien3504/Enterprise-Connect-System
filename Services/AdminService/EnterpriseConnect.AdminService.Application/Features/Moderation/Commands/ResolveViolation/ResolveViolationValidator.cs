using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Commands.ResolveViolation;

public class ResolveViolationValidator : AbstractValidator<ResolveViolationCommand>
{
    public ResolveViolationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
