using FluentValidation;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;

public class CreateGroupConversationValidator : AbstractValidator<CreateGroupConversationCommand>
{
    public CreateGroupConversationValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId không được để trống.");
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Tên nhóm không được để trống.")
            .MaximumLength(250).WithMessage("Tên nhóm tối đa 250 ký tự.");
        RuleFor(x => x.MemberIds)
            .NotNull().WithMessage("Danh sách thành viên không được null.");
    }
}
