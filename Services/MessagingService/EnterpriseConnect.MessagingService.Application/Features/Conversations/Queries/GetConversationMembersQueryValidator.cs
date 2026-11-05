using FluentValidation;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Queries;

public class GetConversationMembersQueryValidator : AbstractValidator<GetConversationMembersQuery>
{
    public GetConversationMembersQueryValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId không được để trống.");

        RuleFor(x => x.CurrentUserId)
            .NotEmpty()
            .WithMessage("CurrentUserId không được để trống.");
    }
}
