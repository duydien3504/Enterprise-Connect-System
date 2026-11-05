using FluentValidation;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.RespondCall;

public class RespondCallValidator : AbstractValidator<RespondCallCommand>
{
    public RespondCallValidator()
    {
        RuleFor(x => x.CallId).NotEmpty().WithMessage("CallId không được rỗng.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId không được rỗng.");
    }
}
