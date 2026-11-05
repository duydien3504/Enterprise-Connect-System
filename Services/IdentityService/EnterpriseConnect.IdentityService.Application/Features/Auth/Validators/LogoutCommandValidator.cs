using FluentValidation;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Validators;

public class LogoutCommandValidator : AbstractValidator<Commands.LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token không được để trống.");
    }
}
