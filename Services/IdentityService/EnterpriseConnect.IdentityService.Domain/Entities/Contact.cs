using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.IdentityService.Domain.Enums;

namespace EnterpriseConnect.IdentityService.Domain.Entities;

public class Contact : BaseEntity
{
    public Guid RequesterId { get; set; }
    public Guid ReceiverId { get; set; }
    public ContactStatus Status { get; set; } = ContactStatus.Pending;

    public virtual User Requester { get; set; } = null!;
    public virtual User Receiver { get; set; } = null!;
}
