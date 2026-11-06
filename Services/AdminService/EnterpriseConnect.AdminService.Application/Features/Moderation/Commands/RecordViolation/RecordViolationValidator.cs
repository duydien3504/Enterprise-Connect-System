using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Commands.RecordViolation;

public class RecordViolationValidator : AbstractValidator<RecordViolationCommand>
{
    public RecordViolationValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.SenderId).NotEmpty();
    }
}
