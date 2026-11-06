using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.System.Commands.UpdateSystemSetting;

public class UpdateSystemSettingValidator : AbstractValidator<UpdateSystemSettingCommand>
{
    public UpdateSystemSettingValidator()
    {
        RuleFor(x => x.SettingKey).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SettingValue).NotNull();
        RuleFor(x => x.GroupName).NotEmpty().MaximumLength(100);
    }
}
