using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;
using EnterpriseConnect.IdentityService.Application.Features.Users.Queries;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Application.Users;

public class GetUsersListQueryHandlerTests
{
    private readonly Mock<IIdentityUnitOfWork> _uowMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly GetUsersListQueryHandler _handler;

    public GetUsersListQueryHandlerTests()
    {
        _uowMock = new Mock<IIdentityUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();

        _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        _handler = new GetUsersListQueryHandler(_uowMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidQuery_ShouldReturnUsersWithMandatoryFullNameAndDepartmentName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var mockUsers = new List<UserListItemDto>
        {
            new(
                Id: userId,
                FullName: "Nguyễn Văn A",
                DepartmentName: "Phòng Công Nghệ Thông Tin",
                DepartmentId: deptId,
                Email: "vana@enterprise.com",
                Position: "Tech Lead",
                AvatarUrl: "https://cloudinary.com/avatar.png",
                Phone: "0901234567"
            )
        };

        _userRepoMock
            .Setup(r => r.GetUsersListAsync(null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockUsers, 1));

        var query = new GetUsersListQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalPages.Should().Be(1);
        result.Items.Should().HaveCount(1);

        var firstUser = result.Items[0];
        firstUser.Id.Should().Be(userId);
        firstUser.FullName.Should().Be("Nguyễn Văn A");
        firstUser.DepartmentName.Should().Be("Phòng Công Nghệ Thông Tin");
        firstUser.DepartmentId.Should().Be(deptId);
        firstUser.Email.Should().Be("vana@enterprise.com");
    }

    [Theory]
    [InlineData(0, 0, 1, 20)]       // page < 1 -> 1, pageSize < 1 -> 20
    [InlineData(-5, -10, 1, 20)]    // negative bounds normalized
    [InlineData(2, 150, 2, 20)]     // pageSize > 100 capped back to 20
    [InlineData(3, 50, 3, 50)]      // valid page and pageSize preserved
    public async Task Handle_WhenPageOrPageSizeOutOfBounds_ShouldNormalizeProperly(
        int inputPage,
        int inputPageSize,
        int expectedPage,
        int expectedPageSize)
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetUsersListAsync(It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), expectedPage, expectedPageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<UserListItemDto>(), 0));

        var query = new GetUsersListQuery(Page: inputPage, PageSize: inputPageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Page.Should().Be(expectedPage);
        result.PageSize.Should().Be(expectedPageSize);
        _userRepoMock.Verify(r => r.GetUsersListAsync(null, null, null, expectedPage, expectedPageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 20, 0)]    // 0 records -> 0 total pages
    [InlineData(20, 20, 1)]   // 20 records with pageSize 20 -> 1 page
    [InlineData(21, 20, 2)]   // 21 records with pageSize 20 -> 2 pages
    [InlineData(55, 10, 6)]   // 55 records with pageSize 10 -> 6 pages
    public async Task Handle_WhenCalculatingTotalPages_ShouldComputeAccurately(
        int totalCount,
        int pageSize,
        int expectedTotalPages)
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetUsersListAsync(null, null, null, 1, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<UserListItemDto>(), totalCount));

        var query = new GetUsersListQuery(Page: 1, PageSize: pageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(totalCount);
        result.TotalPages.Should().Be(expectedTotalPages);
    }

    [Fact]
    public async Task Handle_WhenFilterCriteriaProvided_ShouldPassAllFiltersToRepository()
    {
        // Arrange
        var filterName = "Trần";
        var filterDeptId = Guid.NewGuid();
        var filterDeptName = "Kế toán";

        _userRepoMock
            .Setup(r => r.GetUsersListAsync(filterName, filterDeptId, filterDeptName, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<UserListItemDto>(), 0));

        var query = new GetUsersListQuery(
            Name: filterName,
            DepartmentId: filterDeptId,
            DepartmentName: filterDeptName,
            Page: 1,
            PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _userRepoMock.Verify(r => r.GetUsersListAsync(filterName, filterDeptId, filterDeptName, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoDepartment_ShouldReturnNullForDepartmentName()
    {
        // Arrange
        var mockUsers = new List<UserListItemDto>
        {
            new(
                Id: Guid.NewGuid(),
                FullName: "Lê Văn C",
                DepartmentName: null,
                DepartmentId: null,
                Email: "vanc@enterprise.com",
                Position: "Freelance Consultant",
                AvatarUrl: null,
                Phone: null
            )
        };

        _userRepoMock
            .Setup(r => r.GetUsersListAsync(null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockUsers, 1));

        var query = new GetUsersListQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].FullName.Should().Be("Lê Văn C");
        result.Items[0].DepartmentName.Should().BeNull();
        result.Items[0].DepartmentId.Should().BeNull();
    }
}
