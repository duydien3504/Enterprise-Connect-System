using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.EndCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InitiateCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InviteParticipant;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.RespondCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Queries.GetCallHistory;
using EnterpriseConnect.MediaService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EnterpriseConnect.MediaService.UnitTests;

public class CallValidatorsTests
{
    [Fact]
    public void InitiateCallValidator_ShouldFailWhenFieldsEmpty()
    {
        var validator = new InitiateCallValidator();
        var command = new InitiateCallCommand(Guid.Empty, Guid.Empty, CallType.Voice, CallScope.Direct, new List<Guid>());

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConversationId");
        result.Errors.Should().Contain(e => e.PropertyName == "InitiatorId");
        result.Errors.Should().Contain(e => e.PropertyName == "ParticipantIds");
    }

    [Fact]
    public void RespondCallValidator_ShouldFailWhenCallIdOrUserIdEmpty()
    {
        var validator = new RespondCallValidator();
        var command = new RespondCallCommand(Guid.Empty, Guid.Empty, true);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CallId");
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void EndCallValidator_ShouldFailWhenCallIdOrUserIdEmpty()
    {
        var validator = new EndCallValidator();
        var command = new EndCallCommand(Guid.Empty, Guid.Empty);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CallId");
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void InviteParticipantValidator_ShouldFailWhenUserIdsEmpty()
    {
        var validator = new InviteParticipantValidator();
        var command = new InviteParticipantCommand(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>());

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserIds");
    }

    [Fact]
    public void GetCallHistoryValidator_ShouldValidateRange()
    {
        var validator = new GetCallHistoryValidator();
        var invalidQuery = new GetCallHistoryQuery(Guid.Empty, Skip: -1, Take: 200);

        var result = validator.Validate(invalidQuery);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
        result.Errors.Should().Contain(e => e.PropertyName == "Skip");
        result.Errors.Should().Contain(e => e.PropertyName == "Take");
    }
}
