using EnterpriseConnect.BuildingBlocks.Infrastructure.Persistence;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using EnterpriseConnect.IdentityService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.IdentityService.Infrastructure.Persistence.Repositories;

public class ContactRepository : EfRepository<Contact>, IContactRepository
{
    private readonly IdentityDbContext _identityDbContext;

    public ContactRepository(IdentityDbContext context) : base(context)
    {
        _identityDbContext = context;
    }

    public async Task<Contact?> GetContactBetweenUsersAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c =>
            (c.RequesterId == userA && c.ReceiverId == userB) ||
            (c.RequesterId == userB && c.ReceiverId == userA), cancellationToken);
    }

    public async Task<IReadOnlyList<ContactItemDto>> GetUserContactsAsync(Guid userId, string filterType, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(c => c.RequesterId == userId || c.ReceiverId == userId);

        query = filterType?.ToLower() switch
        {
            "pending_sent" => query.Where(c => c.RequesterId == userId && c.Status == ContactStatus.Pending),
            "pending_received" => query.Where(c => c.ReceiverId == userId && c.Status == ContactStatus.Pending),
            "blocked" => query.Where(c => c.RequesterId == userId && c.Status == ContactStatus.Blocked),
            _ => query.Where(c => c.Status == ContactStatus.Accepted) // default: accepted
        };

        var contacts = await query
            .Include(c => c.Requester).ThenInclude(u => u.Department)
            .Include(c => c.Receiver).ThenInclude(u => u.Department)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return contacts.Select(c =>
        {
            var isRequester = c.RequesterId == userId;
            var target = isRequester ? c.Receiver : c.Requester;

            return new ContactItemDto(
                c.Id,
                target.Id,
                target.FullName,
                target.Email,
                target.AvatarUrl,
                target.Position,
                target.Department?.DepartmentName,
                (int)c.Status,
                isRequester,
                c.CreatedAt
            );
        }).ToList();
    }

    public async Task<(bool IsBlocked, Guid? BlockedBy)> CheckBlockStatusAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default)
    {
        var contact = await GetContactBetweenUsersAsync(userA, userB, cancellationToken);
        if (contact != null && contact.Status == ContactStatus.Blocked)
        {
            return (true, contact.RequesterId); // RequesterId is the one who initiated the block
        }
        return (false, null);
    }
}
