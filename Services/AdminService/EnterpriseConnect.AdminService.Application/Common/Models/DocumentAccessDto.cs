namespace EnterpriseConnect.AdminService.Application.Common.Models;

public class DocumentAccessDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid? RoleId { get; set; }
    public Guid? DepartmentId { get; set; }
}
