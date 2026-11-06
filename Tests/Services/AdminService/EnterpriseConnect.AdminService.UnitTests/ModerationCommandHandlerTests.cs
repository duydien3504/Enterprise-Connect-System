using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Features.Moderation.Commands.RecordViolation;
using EnterpriseConnect.AdminService.Application.Features.Moderation.Commands.ResolveViolation;
using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class ModerationCommandHandlerTests
{
    private readonly Mock<IAdminUnitOfWork> _uowMock = new();
    private readonly Mock<IToxicViolationRepository> _repoMock = new();

    public ModerationCommandHandlerTests()
    {
        _uowMock.Setup(u => u.ToxicViolations).Returns(_repoMock.Object);
    }

    [Fact]
    public async Task RecordViolation_Valid_ShouldSaveAndReturnDto()
    {
        var handler = new RecordViolationCommandHandler(_uowMock.Object);
        var command = new RecordViolationCommand
        {
            MessageId = Guid.NewGuid(),
            SenderId = Guid.NewGuid(),
            ViolationType = ViolationType.PersonalAttack,
            Severity = SeverityLevel.High,
            ActionTaken = ActionTaken.Warned
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.ViolationType.Should().Be(ViolationType.PersonalAttack);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<ToxicViolation>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveViolation_Existing_ShouldUpdateActionTaken()
    {
        var violation = new ToxicViolation { Id = Guid.NewGuid(), ActionTaken = ActionTaken.Warned };
        _repoMock.Setup(r => r.GetByIdAsync(violation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(violation);

        var handler = new ResolveViolationCommandHandler(_uowMock.Object);
        var command = new ResolveViolationCommand { Id = violation.Id, ActionTaken = ActionTaken.Blocked };
        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.ActionTaken.Should().Be(ActionTaken.Blocked);
        _repoMock.Verify(r => r.UpdateAsync(violation, It.IsAny<CancellationToken>()), Times.Once);
    }
}
