using FluentValidation;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Validators;

public class RefreshTokenCommandValidator : AbstractValidator<Commands.RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token không được để trống.");
    }
}
