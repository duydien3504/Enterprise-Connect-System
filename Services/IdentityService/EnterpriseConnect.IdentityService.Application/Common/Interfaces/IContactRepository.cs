using EnterpriseConnect.BuildingBlocks.Application.Contracts;
using EnterpriseConnect.IdentityService.Domain.Entities;

namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public interface IContactRepository : IRepository<Contact>
{
    Task<Contact?> GetContactBetweenUsersAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContactItemDto>> GetUserContactsAsync(Guid userId, string filterType, CancellationToken cancellationToken = default);
    Task<(bool IsBlocked, Guid? BlockedBy)> CheckBlockStatusAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteBetweenUsersAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
}
