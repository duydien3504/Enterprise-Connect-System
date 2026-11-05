using FluentValidation;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;

public class ReactMessageValidator : AbstractValidator<ReactMessageCommand>
{
    public ReactMessageValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty().WithMessage("MessageId không được để trống.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId không được để trống.");
        RuleFor(x => x.Emoji)
            .NotEmpty().WithMessage("Emoji không được để trống.")
            .MaximumLength(32).WithMessage("Emoji tối đa 32 ký tự.");
    }
}
