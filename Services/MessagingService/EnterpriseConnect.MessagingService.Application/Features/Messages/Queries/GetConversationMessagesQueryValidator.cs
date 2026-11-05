using FluentValidation;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Queries;

public class GetConversationMessagesQueryValidator : AbstractValidator<GetConversationMessagesQuery>
{
    public GetConversationMessagesQueryValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage("ConversationId không được rỗng.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId không được rỗng.");
        RuleFor(x => x.Skip).GreaterThanOrEqualTo(0).WithMessage("Skip phải lớn hơn hoặc bằng 0.");
        RuleFor(x => x.Take).InclusiveBetween(1, 100).WithMessage("Take phải từ 1 đến 100.");
    }
}
