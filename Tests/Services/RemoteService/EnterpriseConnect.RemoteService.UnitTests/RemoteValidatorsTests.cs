using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.ApproveSession;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.CreateSession;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.JoinSession;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RequestPermission;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RespondPermission;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RevokePermission;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.TerminateSession;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionByCode;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionHistory;
using EnterpriseConnect.RemoteService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EnterpriseConnect.RemoteService.UnitTests;

public class RemoteValidatorsTests
{
    [Fact]
    public void CreateSessionValidator_ShouldFailWhenRequesterEmptyOrInvalidExpiry()
    {
        var validator = new CreateSessionValidator();
        var command = new CreateSessionCommand(Guid.Empty, ExpirationMinutes: 0);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RequesterId");
        result.Errors.Should().Contain(e => e.PropertyName == "ExpirationMinutes");
    }

    [Fact]
    public void JoinSessionValidator_ShouldFailWhenCodeOrSupporterEmpty()
    {
        var validator = new JoinSessionValidator();
        var command = new JoinSessionCommand("", Guid.Empty);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SessionCode");
        result.Errors.Should().Contain(e => e.PropertyName == "SupporterId");
    }

    [Fact]
    public void ApproveSessionValidator_ShouldFailWhenIdsEmpty()
    {
        var validator = new ApproveSessionValidator();
        var command = new ApproveSessionCommand(Guid.Empty, Guid.Empty, Accept: true);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SessionId");
        result.Errors.Should().Contain(e => e.PropertyName == "RequesterId");
    }

    [Fact]
    public void RequestPermissionValidator_ShouldFailWhenIdsEmpty()
    {
        var validator = new RequestPermissionValidator();
        var command = new RequestPermissionCommand(Guid.Empty, Guid.Empty, PermissionType.ControlInput);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SessionId");
        result.Errors.Should().Contain(e => e.PropertyName == "SupporterId");
    }

    [Fact]
    public void TerminateSessionValidator_ShouldFailWhenIdsEmpty()
    {
        var validator = new TerminateSessionValidator();
        var command = new TerminateSessionCommand(Guid.Empty, Guid.Empty);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SessionId");
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void GetSessionHistoryValidator_ShouldValidateRange()
    {
        var validator = new GetSessionHistoryValidator();
        var query = new GetSessionHistoryQuery(Guid.Empty, Skip: -1, Take: 200);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
        result.Errors.Should().Contain(e => e.PropertyName == "Skip");
        result.Errors.Should().Contain(e => e.PropertyName == "Take");
    }
}
