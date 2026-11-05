using FluentValidation;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Validators;

public class ChangePasswordCommandValidator : AbstractValidator<Commands.ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID không hợp lệ.");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mật khẩu hiện tại không được để trống.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu mới phải có ít nhất 6 ký tự.")
            .NotEqual(x => x.CurrentPassword).WithMessage("Mật khẩu mới không được trùng với mật khẩu cũ.");
    }
}
