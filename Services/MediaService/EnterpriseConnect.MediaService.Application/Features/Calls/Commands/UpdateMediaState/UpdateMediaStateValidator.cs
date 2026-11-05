using FluentValidation;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.UpdateMediaState;

public class UpdateMediaStateValidator : AbstractValidator<UpdateMediaStateCommand>
{
    public UpdateMediaStateValidator()
    {
        RuleFor(x => x.CallId).NotEmpty().WithMessage("CallId không được rỗng.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId không được rỗng.");
    }
}
