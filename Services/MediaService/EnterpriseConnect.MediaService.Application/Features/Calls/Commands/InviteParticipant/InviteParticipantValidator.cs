using FluentValidation;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InviteParticipant;

public class InviteParticipantValidator : AbstractValidator<InviteParticipantCommand>
{
    public InviteParticipantValidator()
    {
        RuleFor(x => x.CallId)
            .NotEmpty().WithMessage("CallId is required.");

        RuleFor(x => x.InviterId)
            .NotEmpty().WithMessage("InviterId is required.");

        RuleFor(x => x.UserIds)
            .NotEmpty().WithMessage("At least one user must be invited.")
            .Must(ids => ids != null && ids.All(id => id != Guid.Empty))
            .WithMessage("All participant IDs must be valid GUIDs.");
    }
}
