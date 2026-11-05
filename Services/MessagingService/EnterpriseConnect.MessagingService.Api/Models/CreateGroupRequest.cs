namespace EnterpriseConnect.MessagingService.Api.Models;

public record CreateGroupRequest(string GroupName, List<Guid> MemberIds);
