using FluentValidation;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;

public class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage("ConversationId không được để trống.");
        RuleFor(x => x.SenderId).NotEmpty().WithMessage("SenderId không được để trống.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung tin nhắn không được để trống.")
            .MaximumLength(4000).WithMessage("Nội dung tin nhắn tối đa 4000 ký tự.");
    }
}
