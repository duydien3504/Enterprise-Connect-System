using FluentValidation;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InitiateCall;

public class InitiateCallValidator : AbstractValidator<InitiateCallCommand>
{
    public InitiateCallValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage("ConversationId không được rỗng.");
        RuleFor(x => x.InitiatorId).NotEmpty().WithMessage("InitiatorId không được rỗng.");
        RuleFor(x => x.ParticipantIds).NotEmpty().WithMessage("Danh sách người tham gia không được rỗng.");
        RuleFor(x => x.CallType).IsInEnum().WithMessage("Loại cuộc gọi không hợp lệ.");
        RuleFor(x => x.Scope).IsInEnum().WithMessage("Phạm vi cuộc gọi không hợp lệ.");
    }
}
