using EnterpriseConnect.BuildingBlocks.Application.Contracts;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Roles.Commands;
using EnterpriseConnect.IdentityService.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Application.Roles;

public class RoleCommandHandlersTests
{
    private readonly Mock<IIdentityUnitOfWork> _uowMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRepository<Role>> _roleRepoMock;
    private readonly Mock<IRepository<Permission>> _permRepoMock;
    private readonly Mock<IRepository<RolePermission>> _rolePermRepoMock;
    private readonly Mock<IRepository<UserRole>> _userRoleRepoMock;

    public RoleCommandHandlersTests()
    {
        _uowMock = new Mock<IIdentityUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _roleRepoMock = new Mock<IRepository<Role>>();
        _permRepoMock = new Mock<IRepository<Permission>>();
        _rolePermRepoMock = new Mock<IRepository<RolePermission>>();
        _userRoleRepoMock = new Mock<IRepository<UserRole>>();

        _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        _uowMock.Setup(u => u.Roles).Returns(_roleRepoMock.Object);
        _uowMock.Setup(u => u.Permissions).Returns(_permRepoMock.Object);
        _uowMock.Setup(u => u.RolePermissions).Returns(_rolePermRepoMock.Object);
        _uowMock.Setup(u => u.UserRoles).Returns(_userRoleRepoMock.Object);
    }

    [Fact]
    public async Task CreateRole_ShouldAddRoleAndAssociatePermissions()
    {
        // Arrange
        var permId = Guid.NewGuid();
        var allPerms = new List<Permission>
        {
            new Permission { Id = permId, PermissionName = "Chat.Read" }
        };

        _permRepoMock.Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allPerms);

        Role? savedRole = null;
        _roleRepoMock.Setup(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .Callback<Role, CancellationToken>((r, _) => savedRole = r);

        var handler = new CreateRoleCommandHandler(_uowMock.Object);

        // Act
        var roleId = await handler.Handle(new CreateRoleCommand("Support", "Support Role", new List<string> { "Chat.Read" }), CancellationToken.None);

        // Assert
        savedRole.Should().NotBeNull();
        savedRole!.RoleName.Should().Be("Support");
        _rolePermRepoMock.Verify(rp => rp.AddAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AssignUserRole_WhenValid_ShouldAddUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _userRepoMock.Setup(u => u.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        _roleRepoMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = roleId, RoleName = "Manager" });

        _userRoleRepoMock.Setup(ur => ur.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserRole>()); // chưa gán

        var handler = new AssignUserRoleCommandHandler(_uowMock.Object);

        // Act
        var result = await handler.Handle(new AssignUserRoleCommand(userId, roleId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _userRoleRepoMock.Verify(ur => ur.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
