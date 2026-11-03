using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.IdentityService.Domain.Entities;

public class Department : BaseEntity
{
    public string DepartmentName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; } // Logical FK to User

    public virtual User? Manager { get; set; }
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
