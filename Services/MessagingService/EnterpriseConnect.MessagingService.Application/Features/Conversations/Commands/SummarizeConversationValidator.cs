using FluentValidation;

namespace EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;

public class SummarizeConversationValidator : AbstractValidator<SummarizeConversationCommand>
{
    public SummarizeConversationValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage("ConversationId không được rỗng.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId không được rỗng.");
        RuleFor(x => x.MessageCount)
            .InclusiveBetween(5, 200).WithMessage("Số lượng tin nhắn tóm tắt phải từ 5 đến 200.");

        RuleFor(x => x.Language)
            .Must(lang => string.IsNullOrWhiteSpace(lang) ||
                          lang.Equals("vi", StringComparison.OrdinalIgnoreCase) ||
                          lang.Equals("en", StringComparison.OrdinalIgnoreCase) ||
                          lang.Equals("vietnamese", StringComparison.OrdinalIgnoreCase) ||
                          lang.Equals("english", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Ngôn ngữ tóm tắt chỉ chấp nhận 'vi' (tiếng Việt) hoặc 'en' (tiếng Anh).");
    }
}
