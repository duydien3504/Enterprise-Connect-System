using FluentValidation;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;

public class CreateDirectConversationValidator : AbstractValidator<CreateDirectConversationCommand>
{
    public CreateDirectConversationValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("UserId hiện tại không được rỗng.");
        RuleFor(x => x.TargetUserId).NotEmpty().WithMessage("TargetUserId người nhận không được rỗng.");
        RuleFor(x => x).Must(x => x.CurrentUserId != x.TargetUserId).WithMessage("Không thể tạo trò chuyện với chính mình.");
    }
}
