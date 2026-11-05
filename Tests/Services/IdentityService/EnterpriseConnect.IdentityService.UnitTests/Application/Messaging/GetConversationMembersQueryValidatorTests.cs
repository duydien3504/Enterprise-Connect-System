using EnterpriseConnect.MessagingService.Application.Features.Conversations.Queries;
using FluentAssertions;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Application.Messaging;

public class GetConversationMembersQueryValidatorTests
{
    [Fact]
    public void Validator_WhenValidQuery_ShouldNotHaveErrors()
    {
        var validator = new GetConversationMembersQueryValidator();
        var query = new GetConversationMembersQuery(Guid.NewGuid(), Guid.NewGuid());

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_WhenEmptyIds_ShouldHaveErrors()
    {
        var validator = new GetConversationMembersQueryValidator();
        var query = new GetConversationMembersQuery(Guid.Empty, Guid.Empty);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}
