using FluentValidation;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.EndCall;

public class EndCallValidator : AbstractValidator<EndCallCommand>
{
    public EndCallValidator()
    {
        RuleFor(x => x.CallId).NotEmpty().WithMessage("CallId không được rỗng.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId không được rỗng.");
    }
}
